using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class CreateDecisionResponse
    {
        public string DecisionId { get; set; }
        public string DecisionCode { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime IssueDate { get; set; }
        public string IssuedByUserId { get; set; }
        public string CertificateId { get; set; }
        public string DecisionTemplateId { get; set; }
        public DecisionStatus DecisionStatus { get; set; }
    }
}
