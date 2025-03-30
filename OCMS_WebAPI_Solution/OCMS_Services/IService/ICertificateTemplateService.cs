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
    public interface ICertificateTemplateService
    {
        Task<CreateCertificateTemplateResponse> CreateCertificateTemplateAsync(CreateCertificateTemplateDTO request, string currentUserId);
        Task<GetCertificateTemplateResponse> GetCertificateTemplateByIdAsync(string templateId);
        Task<GetAllCertificateTemplatesResponse> GetAllCertificateTemplatesAsync();
        Task<bool> DeleteCertificateTemplateAsync(string templateId, string currentUserId);
        Task<UpdateCertificateTemplateResponse> UpdateCertificateTemplateAsync(string templateId, UpdateCertificateTemplateDTO request, string currentUserId);
    }
}
