using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class CertificateModel
    {
        public string CertificateId { get; set; }
        public string CertificateCode { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string TemplateId { get; set; }
        public DateTime IssueDate { get; set; }
        public string Status { get; set; }
        public string CertificateURL { get; set; }
        public string CertificateURLwithSas { get; set; }
        public DateTime? ExpirationDate { get; set; }

    }
}
