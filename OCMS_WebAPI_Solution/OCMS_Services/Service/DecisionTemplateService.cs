using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class DecisionTemplateService : IDecisionTemplateService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private const string CONTAINER_NAME = "decision-templates";

        public DecisionTemplateService(
            UnitOfWork unitOfWork,
            IBlobService blobService,
            IRequestService requestService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _blobService = blobService;
            _requestService = requestService;
            _mapper = mapper;
        }

        #region Create Decision Template
        public async Task<CreateDecisionTemplateResponse> CreateDecisionTemplateAsync(CreateDecisionTemplateDTO request, string currentUserId)
        {
            try
            {
                // Xác định loại template dựa trên templateName
                string templateType = DetermineTemplateType(request.templateName);

                // Sinh ID template mới
                string templateId = await GenerateNextDecisionTemplateIdAsync(templateType);

                // Tạo tên file dựa trên ID template
                string blobName = $"{templateId}.html";

                // Upload template lên Blob Storage
                using (var stream = request.templateContent.OpenReadStream())
                {
                    string templateUrl = await _blobService.UploadFileAsync(CONTAINER_NAME, blobName, stream, "text/html");

                    // Lưu URL gốc (không có SAS token)
                    string baseTemplateUrl = _blobService.GetBlobUrlWithoutSasToken(templateUrl);

                    // Tạo đối tượng template
                    var template = _mapper.Map<DecisionTemplate>(request);
                    template.CreatedByUserId = currentUserId;
                    template.DecisionTemplateId = templateId;
                    template.TemplateContent = baseTemplateUrl;
                    template.CreatedAt = DateTime.Now;
                    template.TemplateStatus = 0; // Draft status

                    // Thêm template vào database
                    await _unitOfWork.DecisionTemplateRepository.AddAsync(template);
                    await _unitOfWork.SaveChangesAsync();

                    // Mapping response
                    var response = _mapper.Map<CreateDecisionTemplateResponse>(template);

                    // Trả về URL có SAS token trong response
                    response.TemplateContentWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(baseTemplateUrl, TimeSpan.FromHours(1));

                    var requestDto = new RequestDTO
                    {
                        RequestType = RequestType.DecisionTemplate,
                        RequestEntityId = template.DecisionTemplateId,
                        Description = $"Request to approve Decision Template {template.DecisionTemplateId}",
                        Notes = "Please review the template and approve as soon as possible.",
                    };
                    await _requestService.CreateRequestAsync(requestDto, currentUserId);

                    return response;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating decision template", ex);
            }
        }
        #endregion

        #region Get Decision Template
        public async Task<DecisionTemplate> GetDecisionTemplateByIdAsync(string templateId)
        {
            var template = await _unitOfWork.DecisionTemplateRepository
                .GetQuery()
                .Include(t => t.CreatedByUser)
                .Include(t => t.ApprovedByUser)
                .FirstOrDefaultAsync(t => t.DecisionTemplateId == templateId);

            if (template == null)
                return null;

            return template;
        }

        public async Task<DecisionTemplateModel> GetDecisionTemplateDetailsByIdAsync(string templateId)
        {
            var template = await GetDecisionTemplateByIdAsync(templateId);

            if (template == null)
                return null;

            // Mapping response sử dụng AutoMapper
            var response = _mapper.Map<DecisionTemplateModel>(template);

            // Thêm SAS token vào URL template file
            response.TemplateContentWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(template.TemplateContent, TimeSpan.FromHours(1));

            return response;
        }
        #endregion

        #region Get All Decision Templates
        public async Task<GetAllDecisionTemplatesResponse> GetAllDecisionTemplatesAsync()
        {
            var templates = await _unitOfWork.DecisionTemplateRepository
                .GetQuery()
                .Include(t => t.CreatedByUser)
                .Include(t => t.ApprovedByUser)
                .ToListAsync();

            return new GetAllDecisionTemplatesResponse
            {
                Templates = _mapper.Map<List<GetAllDecisionTemplatesResponse.DecisionTemplateItem>>(templates)
            };
        }
        #endregion

        #region Get Decision Template by Name
        public async Task<DecisionTemplate> GetDecisionTemplateByNameAsync(string templateName)
        {
            return await _unitOfWork.DecisionTemplateRepository
                .GetQuery()
                .Include(t => t.CreatedByUser)
                .Include(t => t.ApprovedByUser)
                .FirstOrDefaultAsync(t => t.TemplateName.Contains(templateName) && t.TemplateStatus == 1); // Only approved templates
        }
        #endregion

        #region Delete Decision Template
        public async Task<bool> DeleteDecisionTemplateAsync(string templateId, string currentUserId)
        {
            try
            {
                // Tìm template trong database
                var template = await _unitOfWork.DecisionTemplateRepository
                    .GetQuery()
                    .FirstOrDefaultAsync(t => t.DecisionTemplateId == templateId);

                if (template == null)
                    return false;

                // Lưu URL file để xóa khỏi blob storage
                string templateFileUrl = template.TemplateContent;

                // Xóa template khỏi database
                await _unitOfWork.DecisionTemplateRepository.DeleteAsync(templateId);
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

        #region Update Decision Template
        public async Task<UpdateDecisionTemplateResponse> UpdateDecisionTemplateAsync(string templateId, UpdateDecisionTemplateDTO request, string currentUserId)
        {
            try
            {
                var template = await _unitOfWork.DecisionTemplateRepository
                    .GetQuery()
                    .FirstOrDefaultAsync(t => t.DecisionTemplateId == templateId);

                if (template == null)
                    return null;

                template.LastUpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(request.TemplateName))
                {
                    template.TemplateName = request.TemplateName;
                }

                if (!string.IsNullOrEmpty(request.Description))
                {
                    template.Description = request.Description;
                }

                if (request.TemplateContent != null && request.TemplateContent.Length > 0)
                {
                    string oldTemplateUrl = template.TemplateContent;

                    string blobName = $"{templateId}.html";

                    using (var stream = request.TemplateContent.OpenReadStream())
                    {
                        string templateUrl = await _blobService.UploadFileAsync(CONTAINER_NAME, blobName, stream, "text/html");
                        string baseTemplateUrl = _blobService.GetBlobUrlWithoutSasToken(templateUrl);
                        template.TemplateContent = baseTemplateUrl;
                    }
                    await _unitOfWork.SaveChangesAsync();
                    await _blobService.DeleteFileAsync(oldTemplateUrl);
                }
                else
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                var response = _mapper.Map<UpdateDecisionTemplateResponse>(template);
                response.TemplateContentWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(template.TemplateContent, TimeSpan.FromHours(1));

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating decision template", ex);
            }
        }
        #endregion

        #region Helper Methods
        private string DetermineTemplateType(string templateName)
        {
            templateName = templateName.ToLower();

            if (templateName.Contains("initial") || templateName.Contains("khởi đầu"))
                return "INI";

            if (templateName.Contains("recurrent") || templateName.Contains("định kỳ"))
                return "REC";

            // Nếu không xác định được, ném ra ngoại lệ
            throw new ArgumentException("Cannot determine template type from description");
        }

        private async Task<string> GenerateNextDecisionTemplateIdAsync(string templateType)
        {
            // Validate template type
            if (!new[] { "INI", "REC" }.Contains(templateType))
            {
                throw new ArgumentException("Invalid template type. Must be INI or REC.");
            }

            // Tìm template có ID lớn nhất của loại này
            var lastTemplate = await _unitOfWork.DecisionTemplateRepository
                .GetQuery()
                .Where(t => t.DecisionTemplateId.StartsWith($"TEMP-DEC-{templateType}-"))
                .OrderByDescending(t => t.DecisionTemplateId)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (lastTemplate != null)
            {
                // Trích xuất số thứ tự từ ID cuối cùng
                string lastIdNumber = lastTemplate.DecisionTemplateId.Split('-')[3];
                nextSequence = int.Parse(lastIdNumber) + 1;
            }

            // Sinh ID mới
            return $"TEMP-DEC-{templateType}-{nextSequence:D3}";
        }
        #endregion
    }
}