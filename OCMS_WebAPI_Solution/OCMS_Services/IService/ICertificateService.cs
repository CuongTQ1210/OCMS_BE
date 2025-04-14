using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ICertificateService
    {
        
        Task<List<CertificateModel>> AutoGenerateCertificatesForPassedTraineesAsync(string courseId, string issuedByUserId);
        Task<CertificateModel> GetCertificateByIdAsync(string certificateId);
        Task<List<CertificateModel>> GetPendingCertificatesWithSasUrlAsync();
        Task<List<CertificateModel>> GetCertificatesByUserIdWithSasUrlAsync(string userId);
        Task<List<CertificateModel>> GetActiveCertificatesWithSasUrlAsync();
    }
}
