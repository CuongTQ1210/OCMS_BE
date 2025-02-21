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

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey("CertificateTemplate")]
        public int CertificateTemplateId { get; set; }
        public CertificateTemplate CertificateTemplate { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public CertificateStatus Status { get; set; } // active, expired, revoked, returned
        public string DigitalSignature { get; set; }
        public bool IsRevoked { get; set; }
        public string RevocationReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
