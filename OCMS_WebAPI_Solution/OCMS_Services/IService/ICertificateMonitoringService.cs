using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ICertificateMonitoringService
    {
        Task CheckAndNotifyExpiringCertificatesAsync();
        Task CheckAndNotifySingleCertificateAsync(string certificateId);
    }
}
