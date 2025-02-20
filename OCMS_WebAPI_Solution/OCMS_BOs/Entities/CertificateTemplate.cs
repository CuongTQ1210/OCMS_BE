using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class CertificateTemplate
    {
        [Key]
        public string CertificateTemplateId { get; set; }

        public string TemplateName { get; set; }
        public string TemplateFile { get; set; } // Path to PDF

        [ForeignKey("User")]
        public string CreatedBy { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
