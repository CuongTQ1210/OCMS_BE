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
        public string DecisionId     { get; set; }
        public string DecisionCode { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime IssueDate { get; set; }
        public string IssueByUserId {  get; set; }
        [ForeignKey("Certificate")]
        public string CertificateId { get; set; }
        public Certificate Certificate { get; set; }
        [ForeignKey("DigitalSignature")]
        public string DigitalSignatureId { get; set; }
        public DigitalSignature DigitalSignature { get; set; } 
        public DateTime SignDate { get; set; }
        public DecisionStatus DecisionStatus { get; set; }

    }
}
