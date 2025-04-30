using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class CertificateRenewalHistoryModel
    {
        public string CertificateId { get; set; }
        public string CertificateCode { get; set; }
        public DateTime OriginalIssueDate { get; set; }
        public DateTime CurrentIssueDate { get; set; }
        public DateTime? CurrentExpirationDate { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string IssuedByUserId { get; set; }
        public string IssuedByUserName { get; set; }
        public List<RenewalEventModel> RenewalHistory { get; set; } = new List<RenewalEventModel>();
    }
}
