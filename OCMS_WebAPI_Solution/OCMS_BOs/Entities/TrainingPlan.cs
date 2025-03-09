using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TrainingPlan
    {
        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public string Desciption { get; set; }
        public PlanLevel PlanLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [ForeignKey("User")]
        public string CreateByUserId { get; set; }
        public User CreateByUser { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifyDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string? ApproveByUserId { get; set; }
        public User? ApproveByUser { get; set; }
        public DateTime? ApproveDate { get; set; } = DateTime.UtcNow;

        public TrainingPlanStatus TrainingPlanStatus { get; set; }
    }
}
