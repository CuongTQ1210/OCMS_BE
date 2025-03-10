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
        public string Description { get; set; }
        public string TemplateFile { get; set; } // Path to PDF

        [ForeignKey("CreateUser")]
        public string CreatedByUserId { get; set; }
        public User CreateByUser { get; set; }
        public TemplateStatus templateStatus { get; set; }
        [ForeignKey("ApproveUser")]
        public string? ApprovedByUserId { get; set; }   
        public User? ApprovedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; }
    }
}
