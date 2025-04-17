using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class DecisionModel
    {
        public string DecisionId { get; set; }
        public string DecisionCode { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ContentWithSas { get; set; }
        public DateTime IssueDate { get; set; }
        public string IssuedBy { get; set; }
        public DecisionStatus Status { get; set; }
        public string DecisionTemplateId { get; set; }
    }
}
