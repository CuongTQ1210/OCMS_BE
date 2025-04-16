using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Decision
    {
        [Key]
        public string DecisionId { get; set; }

        [Required]
        [StringLength(255)]
        public string DecisionCode { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [ForeignKey("IssuedByUser")]
        public string IssuedByUserId { get; set; }
        public User IssuedByUser { get; set; }

        [ForeignKey("Certificate")]
        public string CertificateId { get; set; }
        public Certificate Certificate { get; set; }

        [ForeignKey("DecisionTemplate")]
        public string DecisionTemplateId { get; set; }
        public DecisionTemplate DecisionTemplate { get; set; }

        [ForeignKey("DigitalSignature")]
        public string? DigitalSignatureId { get; set; }
        public DigitalSignature? DigitalSignature { get; set; }

        [Required]
        public DateTime SignDate { get; set; }

        
        public DecisionStatus DecisionStatus { get; set; }
    }
}
