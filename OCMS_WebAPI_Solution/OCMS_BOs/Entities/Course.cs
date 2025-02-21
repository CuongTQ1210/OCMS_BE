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
        public CourseType CourseType { get; set; } // Initial, Relearn, Recurrent
        public CourseStatus Status { get; set; } // pending, approved, rejected
        public Progress Progress { get; set; } //Ongoing, Completed
        public DateTime? ApprovalDate { get; set; }

        [ForeignKey("User")]
        public string InstructorId { get; set; }
        public User Instructor { get; set; }

        [ForeignKey("User")]
        public string CreatedBy { get; set; }
        public User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
