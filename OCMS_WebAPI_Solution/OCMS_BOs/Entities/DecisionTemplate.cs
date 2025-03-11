using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class DecisionTemplate
    {
        [Key]
        public string DecisionTemplateId { get; set; }

        [Required]
        public string TemplateName { get; set; }

        [Required]
        public string TemplateContent { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("CreatedByUser")]
        public string CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        [ForeignKey("ApprovedByUser")]
        public string ApprovedByUserId { get; set; }
        public User ApprovedByUser { get; set; }

       
        public DateTime? LastUpdatedAt { get; set; } = DateTime.UtcNow;

       
        public int TemplateStatus { get; set; } = 0;
    }
}
