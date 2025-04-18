using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class ExpiredCertificateReportDto
    {
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string Status { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CertificateUrlWithSas { get; set; } 
    }
}
