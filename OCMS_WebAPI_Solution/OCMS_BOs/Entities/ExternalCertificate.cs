using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class ExternalCertificate
    {
        [Key]
        public int ExternalCertificateId { get; set; }

        public string CertificateCode { get; set; }
        public string CertificateName { get; set; }
        public string IssuingOrganization { get; set; }
        [ForeignKey("Candidate")]
        public string? CandidateId { get; set; }
        public Candidate? Candidate { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("VerifyUser")]
        public string VerifyByUserId { get; set; } 
        public User VerifyByUser { get; set; }
        
        public DateTime VerifyDate { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        [Required]
        public string CertificateFileURL { get; set; } // Path to uploaded file

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
