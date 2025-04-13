using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class CertificateTemplateService : ICertificateTemplateService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;
        private readonly IMapper _mapper;

        public CertificateTemplateService(
            UnitOfWork unitOfWork,
            IBlobService blobService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _blobService = blobService;
            _mapper = mapper;
        }

        #region Import Certificate Template
        public async Task<CreateCertificateTemplateResponse> CreateCertificateTemplateAsync(CreateCertificateTemplateDTO request, string currentUserId)
        {
            // Xác định loại template dựa trên description
            string templateType = DetermineTemplateType(request.Description);

            // Sinh ID template mới
            string templateId = await GenerateNextTemplateIdAsync(templateType);

            // Sinh tên template
            string templateName = GenerateTemplateNameByType(templateType, request.Description);

            // Tạo tên file dựa trên ID template
            string blobName = $"{templateId}.html";

            // Upload template lên Blob Storage
            using (var stream = request.HtmlTemplate.OpenReadStream())
            {
                string templateUrl = await _blobService.UploadFileAsync("certificate-templates", blobName, stream, "text/html");

                // Lưu URL gốc (không có SAS token)
                string baseTemplateUrl = _blobService.GetBlobUrlWithoutSasToken(templateUrl);

                // Tạo đối tượng template
                var template = _mapper.Map<CertificateTemplate>(request);
                template.CreatedByUserId = currentUserId;
                template.CertificateTemplateId = templateId;
                template.TemplateFile = baseTemplateUrl;
                template.TemplateName = templateName;

                // Thêm template vào database
                await _unitOfWork.CertificateTemplateRepository.AddAsync(template);
                await _unitOfWork.SaveChangesAsync();

                // Mapping response
                var response = _mapper.Map<CreateCertificateTemplateResponse>(template);

                // Trả về URL có SAS token trong response
                response.TemplateFileWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(baseTemplateUrl, TimeSpan.FromHours(1));

                return response;
            }
        }
        #endregion

        #region Get Certificate Template
        public async Task<GetCertificateTemplateResponse> GetCertificateTemplateByIdAsync(string templateId)
        {
            var template = await _unitOfWork.CertificateTemplateRepository
                .GetQuery()
                .Include(t => t.CreateByUser)
                .Include(t => t.ApprovedByUser)
                .FirstOrDefaultAsync(t => t.CertificateTemplateId == templateId);

            if (template == null)
                return null;

            // Mapping response sử dụng AutoMapper
            var response = _mapper.Map<GetCertificateTemplateResponse>(template);

            // Thêm SAS token vào URL template file
            response.TemplateFileWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(template.TemplateFile, TimeSpan.FromHours(1));

            return response;
        }
        #endregion

        #region Get All Certificate Templates
        public async Task<GetAllCertificateTemplatesResponse> GetAllCertificateTemplatesAsync()
        {
            var templates = await _unitOfWork.CertificateTemplateRepository.GetAllAsync();

            return new GetAllCertificateTemplatesResponse
            {
                Templates = _mapper.Map<List<GetAllCertificateTemplatesResponse.CertificateTemplateItem>>(templates)
            };
        }
        #endregion

        #region Delete Certificate Template
        public async Task<bool> DeleteCertificateTemplateAsync(string templateId, string currentUserId)
        {
            try
            {
                // Tìm template trong database
                var template = await _unitOfWork.CertificateTemplateRepository
                    .GetQuery()
                    .FirstOrDefaultAsync(t => t.CertificateTemplateId == templateId);

                if (template == null)
                    return false;

                // Lưu URL file để xóa khỏi blob storage
                string templateFileUrl = template.TemplateFile;

                // Xóa template khỏi database - truyền templateId vào DeleteAsync
                await _unitOfWork.CertificateTemplateRepository.DeleteAsync(templateId);
                await _unitOfWork.SaveChangesAsync();

                // Xóa file từ blob storage
                await _blobService.DeleteFileAsync(templateFileUrl);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Update Certificate Template
        public async Task<UpdateCertificateTemplateResponse> UpdateCertificateTemplateAsync(string templateId, UpdateCertificateTemplateDTO request, string currentUserId)
        {
            try
            {
                var template = await _unitOfWork.CertificateTemplateRepository
                    .GetQuery()
                    .FirstOrDefaultAsync(t => t.CertificateTemplateId == templateId);

                if (template == null)
                    return null;

                template.LastUpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(request.Description))
                {
                    template.Description = request.Description;

                    string templateType = template.CertificateTemplateId.Split('-')[1]; // Lấy loại từ ID (INI, REC, PRO)
                    template.TemplateName = GenerateTemplateNameByType(templateType, request.Description);
                }

                if (request.TemplateStatus.HasValue)
                {
                    template.templateStatus = request.TemplateStatus.Value;
                }

                if (request.HtmlTemplate != null && request.HtmlTemplate.Length > 0)
                {
                    string oldTemplateUrl = template.TemplateFile;

                    string blobName = $"{templateId}.html";

                    using (var stream = request.HtmlTemplate.OpenReadStream())
                    {
                        string templateUrl = await _blobService.UploadFileAsync("certificate-templates", blobName, stream, "text/html");
                        template.TemplateFile = templateUrl;
                    }
                    await _unitOfWork.SaveChangesAsync();
                    await _blobService.DeleteFileAsync(oldTemplateUrl);
                }
                else
                {
                    await _unitOfWork.SaveChangesAsync();
                }
                return _mapper.Map<UpdateCertificateTemplateResponse>(template);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating certificate template", ex);
            }
        }
        #endregion

        #region Helper Methods
        // Các phương thức khác như DetermineTemplateType và GenerateNextTemplateIdAsync giữ nguyên
        private string DetermineTemplateType(string description)
        {
            description = description.ToLower();

            if (description.Contains("initial") || description.Contains("khởi đầu"))
                return "INI";

            if (description.Contains("recurrent") || description.Contains("định kỳ"))
                return "REC";

            if (description.Contains("professional") || description.Contains("chuyên nghiệp"))
                return "PRO";

            // Nếu không xác định được, ném ra ngoại lệ
            throw new ArgumentException("Cannot determine template type from description");
        }

        private async Task<string> GenerateNextTemplateIdAsync(string templateType)
        {
            // Validate template type
            if (!new[] { "INI", "REC", "PRO" }.Contains(templateType))
            {
                throw new ArgumentException("Invalid template type. Must be INI, REC, or PRO.");
            }

            // Tìm template có ID lớn nhất của loại này
            var lastTemplate = await _unitOfWork.CertificateTemplateRepository
                .GetQuery()
                .Where(t => t.CertificateTemplateId.StartsWith($"TEMP-{templateType}-"))
                .OrderByDescending(t => t.CertificateTemplateId)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (lastTemplate != null)
            {
                // Trích xuất số thứ tự từ ID cuối cùng
                string lastIdNumber = lastTemplate.CertificateTemplateId.Split('-')[2];
                nextSequence = int.Parse(lastIdNumber) + 1;
            }

            // Sinh ID mới
            return $"TEMP-{templateType}-{nextSequence:D3}";
        }

        private string GenerateTemplateNameByType(string templateType, string description)
        {
            return templateType switch
            {
                "INI" => $"Initial Certificate Template - {description}",
                "REC" => $"Recurrent Certificate Template - {description}",
                "PRO" => $"Professional Certificate Template - {description}",
                _ => "Unnamed Certificate Template"
            };
        }
        #endregion
    }
}