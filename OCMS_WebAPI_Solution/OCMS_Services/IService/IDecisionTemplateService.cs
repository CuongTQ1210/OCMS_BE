using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IDecisionTemplateService
    {
        Task<CreateDecisionTemplateResponse> CreateDecisionTemplateAsync(CreateDecisionTemplateDTO dto, string userId);
        Task<bool> DeleteDecisionTemplateAsync(string templateId);
        Task<IEnumerable<DecisionTemplate>> GetAllDecisionTemplatesAsync();
        Task<DecisionTemplate> GetDecisionTemplateByIdAsync(string templateId);
        Task<DecisionTemplate> GetDecisionTemplateByNameAsync(string templateName);
        Task<string> GetTemplateContentAsync(string templateId);        
        Task<UpdateDecisionTemplateResponse> UpdateDecisionTemplateAsync(string templateId, UpdateDecisionTemplateDTO dto);
    }
}
