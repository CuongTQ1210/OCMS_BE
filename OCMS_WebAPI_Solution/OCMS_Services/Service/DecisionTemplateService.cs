using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class DecisionTemplateService : IDecisionTemplateService
    {
        private readonly IBlobService _blobService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IDecisionTemplateRepository _decisionTemplateRepository;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private const string CONTAINER_NAME = "decision-templates";

        public DecisionTemplateService(
            IBlobService blobService,
            UnitOfWork unitOfWork,
            IDecisionTemplateRepository decisionTemplateRepository,
            IRequestService requestService,
            IMapper mapper)
        {
            _blobService = blobService;
            _unitOfWork = unitOfWork;
            _decisionTemplateRepository = decisionTemplateRepository;
            _requestService = requestService;
            _mapper = mapper;
        }

        #region Create Decision Template
        public async Task<CreateDecisionTemplateResponse> CreateDecisionTemplateAsync(CreateDecisionTemplateDTO dto, string userId)
        {
            try
            {
                if (dto == null)
                    throw new Exception("Decision template data cannot be null");
                if (string.IsNullOrEmpty(dto.templateName))
                    throw new Exception("Template name is required");
                if (dto.templateContent == null)
                    throw new Exception("Template content is required");

                // Determine template type and generate ID based on description
                var templateType = DetermineTemplateType(dto.templateName);
                var templateId = await GenerateNextDecisionTemplateIdAsync(templateType);
                var blobName = $"{templateId}.html";

                // Create entity and map from DTO using AutoMapper
                var decisionTemplate = _mapper.Map<DecisionTemplate>(dto);

                // Set properties that are not part of the mapping
                decisionTemplate.DecisionTemplateId = templateId;
                decisionTemplate.CreatedAt = DateTime.Now;
                decisionTemplate.CreatedByUserId = userId;
                
                // Explicitly set ApprovedByUserId to null to handle not-null constraint in database
                decisionTemplate.TemplateStatus = 0; // Draft status

                // Upload template content to blob storage
                using (var stream = dto.templateContent.OpenReadStream())
                {
                    var blobUrl = await _blobService.UploadFileAsync(CONTAINER_NAME, blobName, stream, "text/html");

                    // Set the blob URL after upload
                    decisionTemplate.TemplateContent = blobUrl;

                    // Save to database
                    await _unitOfWork.DecisionTemplateRepository.AddAsync(decisionTemplate);
                    await _unitOfWork.SaveChangesAsync();

                    // Generate SAS URL for template content if needed
                    string templateContentWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(blobUrl, TimeSpan.FromHours(1));

                    // Create response model using AutoMapper
                    var response = _mapper.Map<CreateDecisionTemplateResponse>(decisionTemplate);
                    
                    // Set the SAS URL which is not part of the entity mapping
                    response.TemplateContentWithSas = templateContentWithSas;

                    var requestDto = new RequestDTO
                    {
                        RequestType = RequestType.DecisionTemplate,
                        RequestEntityId = decisionTemplate.DecisionTemplateId,
                        Description = $"Request to approve Decision Template {decisionTemplate.DecisionTemplateId}",
                        Notes = "Please review the template and approve as soon as possible.",
                    };
                    await _requestService.CreateRequestAsync(requestDto, userId);

                    return response;
                }
            }
            catch (Exception ex)
            {
                // Log the exception here
                throw new Exception("Error creating decision template: " + ex.Message);
            }
        }
        #endregion

        #region Delete Decision Template
        public async Task<bool> DeleteDecisionTemplateAsync(string templateId)
        {
            var template = await _unitOfWork.DecisionTemplateRepository.FirstOrDefaultAsync(t => t.DecisionTemplateId == templateId);

            if (template == null)
                return false;

            // Delete the blob if it exists
            if (!string.IsNullOrEmpty(template.TemplateContent))
            {
                await _blobService.DeleteFileAsync(template.TemplateContent);
            }

            await _unitOfWork.DecisionTemplateRepository.DeleteAsync(template.DecisionTemplateId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Get all Decision Template
        public async Task<IEnumerable<DecisionTemplate>> GetAllDecisionTemplatesAsync()
        {
            return await _unitOfWork.DecisionTemplateRepository.GetAllAsync(
                includeProperties: "CreatedByUser,ApprovedByUser");
        }
        #endregion

        #region Get Decision Template by ID
        public async Task<DecisionTemplate> GetDecisionTemplateByIdAsync(string templateId)
        {
            return await _unitOfWork.DecisionTemplateRepository.GetAsync(
                t => t.DecisionTemplateId == templateId,
                t => t.CreatedByUser,
                t => t.ApprovedByUser);
        }
        #endregion

        #region Get Decision Template by Name
        public async Task<DecisionTemplate> GetDecisionTemplateByNameAsync(string templateName)
        {
            return await _unitOfWork.DecisionTemplateRepository.GetAsync(
                t => t.TemplateName.Contains(templateName) && t.TemplateStatus == 1, // Only approved templates
                t => t.CreatedByUser,
                t => t.ApprovedByUser);
        }
        #endregion

        #region Get Decision Template link
        public async Task<string> GetTemplateContentAsync(string templateId)
        {
            var template = await GetDecisionTemplateByIdAsync(templateId);
            if (template == null)
                throw new Exception("Template not found");

            // Check if the content is a blob URL
            if (template.TemplateContent.StartsWith("http"))
            {
                // Generate SAS token for reading the blob
                var sasUrl = await _blobService.GetBlobUrlWithSasTokenAsync(template.TemplateContent, TimeSpan.FromHours(1), "r");

                // Download the content
                using (var httpClient = new HttpClient())
                {
                    return await httpClient.GetStringAsync(sasUrl);
                }
            }

            // If it's not a URL, assume it's the content itself
            return template.TemplateContent;
        }
        #endregion

        #region Update Decision Template
        public async Task<UpdateDecisionTemplateResponse> UpdateDecisionTemplateAsync(string templateId, UpdateDecisionTemplateDTO dto)
        {
            try
            {
                var template = await _unitOfWork.DecisionTemplateRepository.FirstOrDefaultAsync(t => t.DecisionTemplateId == templateId);

                if (template == null)
                    throw new Exception($"Decision template with ID {templateId} not found");

                // Update template properties using AutoMapper
                _mapper.Map(dto, template);
                
                // Set properties not included in mapping
                template.LastUpdatedAt = DateTime.Now;

                // Handle the template content file if provided
                if (dto.TemplateContent != null && dto.TemplateContent.Length > 0)
                {
                    string blobName = $"{templateId}.html";

                    using (var stream = dto.TemplateContent.OpenReadStream())
                    {
                        string templateUrl = await _blobService.UploadFileAsync(CONTAINER_NAME, blobName, stream, "text/html");
                        
                        // Delete old blob if needed
                        if (!string.IsNullOrEmpty(template.TemplateContent))
                        {
                            await _blobService.DeleteFileAsync(template.TemplateContent);
                        }
                        
                        template.TemplateContent = templateUrl;
                    }
                }

                // Update other fields if they are present in the DTO
                if (!string.IsNullOrEmpty(dto.TemplateName))
                    template.TemplateName = dto.TemplateName;

                if (!string.IsNullOrEmpty(dto.Description))
                    template.Description = dto.Description;

                await _unitOfWork.SaveChangesAsync();

                // Map the updated entity to response
                var response = _mapper.Map<UpdateDecisionTemplateResponse>(template);

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception here
                throw new Exception("Error updating decision template: " + ex.Message);
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
                throw new ArgumentException("Invalid template type. Must be INI, REC.");
            }

            // Tìm template có ID lớn nhất của loại này
            var lastTemplate = await _unitOfWork.DecisionTemplateRepository
                .GetQuery()
                .Where(t => t.DecisionTemplateId.StartsWith($"TEMP-{templateType}-"))
                .OrderByDescending(t => t.DecisionTemplateId)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (lastTemplate != null)
            {
                // Trích xuất số thứ tự từ ID cuối cùng
                string lastIdNumber = lastTemplate.DecisionTemplateId.Split('-')[2];
                nextSequence = int.Parse(lastIdNumber) + 1;
            }

            // Sinh ID mới
            return $"TEMP-DEC-{templateType}-{nextSequence:D3}";
        }
        #endregion
    }
}