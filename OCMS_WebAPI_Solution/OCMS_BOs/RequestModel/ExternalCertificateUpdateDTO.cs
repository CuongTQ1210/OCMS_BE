using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class ExternalCertificateUpdateDTO
    {
        public string CertificateCode { get; set; }
        public string CertificateName { get; set; }
        public string IssuingOrganization { get; set; }
    }
}
