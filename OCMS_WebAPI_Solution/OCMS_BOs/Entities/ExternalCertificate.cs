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

        [ForeignKey("User")]
        public string UserId { get; set; } // Trainee
        public User User { get; set; }

        [Required]
        public string CertificateFile { get; set; } // Path to uploaded file

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
