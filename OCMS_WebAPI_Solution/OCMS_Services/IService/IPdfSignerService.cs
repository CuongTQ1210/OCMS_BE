using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IPdfSignerService
    {
        Task<byte[]> SignPdfAsync(string certificateId, string signedByUserId);
        Task SendCertificateByEmailAsync(string certificateId);
        Task<byte[]> SignDecisionAsync(string decisionId);
    }
}
