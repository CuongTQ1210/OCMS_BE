using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendCertificateEmailAsync(string to, string subject, string body, byte[] attachment = null, string attachmentFileName = null);
    }
}
