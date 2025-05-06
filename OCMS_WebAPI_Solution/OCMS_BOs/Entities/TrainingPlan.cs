using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TrainingPlan
    {
        [Key]
        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [ForeignKey("CreateUser")]
        public string CreateByUserId { get; set; }
        public User CreateByUser { get; set; }

        // Reference to Course (n-1 relationship)
        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey("Specialty")]
        public string SpecialtyId { get; set; }
        public Specialties Specialty { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime ModifyDate { get; set; } = DateTime.Now;

        [ForeignKey("ApproveUser")]
        public string? ApproveByUserId { get; set; }
        public User? ApproveByUser { get; set; }
        public DateTime? ApproveDate { get; set; }

        public TrainingPlanStatus TrainingPlanStatus { get; set; }

        // A TrainingPlan can have multiple schedules
        public virtual ICollection<TrainingSchedule> Schedules { get; set; }
    }
}
