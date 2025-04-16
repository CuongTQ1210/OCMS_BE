using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class GetDecisionTemplateResponse
    {
        public string DecisionTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string TemplateContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public string ApprovedByUserId { get; set; }
        public string ApprovedByUserName { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int TemplateStatus { get; set; }
        public string TemplateContentWithSas { get; set; }
    }

    public class GetAllDecisionTemplatesResponse
    {
        public List<DecisionTemplateItem> Templates { get; set; } = new List<DecisionTemplateItem>();

        public class DecisionTemplateItem
        {
            public string DecisionTemplateId { get; set; }
            public string TemplateName { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedByUserName { get; set; }
            public int TemplateStatus { get; set; }
        }
    }
}
