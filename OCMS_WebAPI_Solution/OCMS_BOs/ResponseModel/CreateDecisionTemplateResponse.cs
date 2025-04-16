using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class CreateDecisionTemplateResponse
    {
        public string DecisionTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string TemplateContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public int TemplateStatus { get; set; }
        public string TemplateContentWithSas { get; set; }
    }
}
