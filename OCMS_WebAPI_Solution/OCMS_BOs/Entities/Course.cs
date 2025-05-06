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
        public string CourseName { get; set; }
        public string? Description { get; set; }
        public CourseLevel CourseLevel { get; set; } // Initial, Relearn, Recurrent
        public CourseStatus Status { get; set; } // pending, approved, rejected
        public Progress Progress { get; set; } // Ongoing, Completed

        [ForeignKey("ApproveUser")]
        public string? ApproveByUserId { get; set; }
        public User? ApproveByUser { get; set; }
        public DateTime? ApprovalDate { get; set; }

        [ForeignKey("CreateUser")]
        public string CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Self-reference to related course
        [ForeignKey("RelatedCourse")]
        public string? RelatedCourseId { get; set; }
        public Course? RelatedCourse { get; set; }
        public ICollection<Course> RelatedCourses { get; set; }

        // Course can be referenced by multiple TrainingPlans (1-n)
        public virtual ICollection<TrainingPlan> TrainingPlans { get; set; }

        // Course can have multiple subject-specialty combinations
        public List<CourseSubjectSpecialty> CourseSubjectSpecialties { get; set; }
    }
}
