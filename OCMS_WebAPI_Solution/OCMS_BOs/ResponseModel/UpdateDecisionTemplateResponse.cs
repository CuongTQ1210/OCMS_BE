using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class UpdateDecisionTemplateResponse
    {
        public string DecisionTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int TemplateStatus { get; set; }
    }
}
