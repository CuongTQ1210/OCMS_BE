using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IDecisionTemplateService
    {
        Task<CreateDecisionTemplateResponse> CreateDecisionTemplateAsync(CreateDecisionTemplateDTO request, string currentUserId);
        Task<DecisionTemplateModel> GetDecisionTemplateDetailsByIdAsync(string templateId);
        Task<DecisionTemplate> GetDecisionTemplateByIdAsync(string templateId);
        Task<DecisionTemplate> GetDecisionTemplateByNameAsync(string templateName);
        Task<GetAllDecisionTemplatesResponse> GetAllDecisionTemplatesAsync();
        Task<bool> DeleteDecisionTemplateAsync(string templateId, string currentUserId);
        Task<UpdateDecisionTemplateResponse> UpdateDecisionTemplateAsync(string templateId, UpdateDecisionTemplateDTO request, string currentUserId);
    }
}
