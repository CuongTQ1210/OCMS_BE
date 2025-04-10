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
        Task<CertificateModel> CreateCertificateAsync(CreateCertificateDTO request, string issuedByUserId);
        Task<List<CertificateModel>> GetCertificatesByTraineeIdAsync(string traineeId);
        Task<bool> DeleteCertificateAsync(string certificateId);
    }
}
