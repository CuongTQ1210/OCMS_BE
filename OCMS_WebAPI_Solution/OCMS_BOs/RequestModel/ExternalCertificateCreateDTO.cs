using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class ExternalCertificateCreateDTO
    {

        public string CertificateCode { get; set; }
        public string CertificateName { get; set; }
        public string IssuingOrganization { get; set; }
        public string? CandidateId { get; set; }
    }
}
