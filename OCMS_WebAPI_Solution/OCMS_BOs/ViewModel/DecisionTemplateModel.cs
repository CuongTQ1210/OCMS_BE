using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class DecisionTemplateModel
    {
        public string DecisionTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string TemplateContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public string? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int TemplateStatus { get; set; }
        public string TemplateStatusText => TemplateStatus == 1 ? "Approved" : "Draft";

        // Static method to convert from entity to view model
        public static DecisionTemplateModel FromEntity(DecisionTemplate entity)
        {
            if (entity == null) return null;

            return new DecisionTemplateModel
            {
                DecisionTemplateId = entity.DecisionTemplateId,
                TemplateName = entity.TemplateName,
                Description = entity.Description,
                TemplateContent = entity.TemplateContent,
                CreatedAt = entity.CreatedAt,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByUserName = entity.CreatedByUser?.FullName ?? "Unknown",
                ApprovedByUserId = entity.ApprovedByUserId,
                ApprovedByUserName = entity.ApprovedByUser?.FullName ?? "Not Approved",
                LastUpdatedAt = entity.LastUpdatedAt,
                TemplateStatus = entity.TemplateStatus
            };
        }
    }

    public class DecisionTemplateListItemModel
    {
        public string DecisionTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public int TemplateStatus { get; set; }
        public string TemplateStatusText => TemplateStatus == 1 ? "Approved" : "Draft";
    }
}
