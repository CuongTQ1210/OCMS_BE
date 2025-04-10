using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Certificate
    {
        [Key]
        public string CertificateId { get; set; }
        public string CertificateCode { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey("CertificateTemplate")]
        public string CertificateTemplateId { get; set; }
        public CertificateTemplate CertificateTemplate { get; set; }
        [ForeignKey("IssueUser")]
        public string IssueByUserId { get; set; }
        public User IssueByUser { get; set; }

        public DateTime IssueDate { get; set; }

        [ForeignKey("ApproveUser")]
        public string? ApprovebyUserId { get; set; }
        public User? ApprovebyUser { get; set; }

       
        public CertificateStatus Status { get; set; } // active, expired, revoked, returned
        [ForeignKey("DigitalSignature")]
        public string? DigitalSignatureId { get; set; }
        public DigitalSignature DigitalSignature { get; set; }
        public DateTime SignDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpirationDate { get; set; }

        public string CertificateURL { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevocationReason { get; set; }
       

        public DateTime? RevocationDate { get; set; }
    }
}


