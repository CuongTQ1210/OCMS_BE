using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Course
    {
        [Key]
        public string CourseId { get; set; }
        [ForeignKey("TrainingPlan")]
        public string TrainingPlanId { get; set; }
        public TrainingPlan TrainingPlan { get; set; }
        public string CourseName { get; set; }
        public CourseLevel CourseLevel { get; set; } // Initial, Relearn, Recurrent
        public CourseStatus Status { get; set; } // pending, approved, rejected
        public Progress Progress { get; set; } //Ongoing, Completed
        [ForeignKey("ApproveUser")]
        public string? ApproveByUserId { get; set; }
        public User? ApproveByUser { get; set; }
        public DateTime? ApprovalDate { get; set; }


        [ForeignKey("CreateUser")]
        public string CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<Subject> Subjects { get; set; }
        public List<TraineeAssign> Trainees { get;   set;}
    }
}
