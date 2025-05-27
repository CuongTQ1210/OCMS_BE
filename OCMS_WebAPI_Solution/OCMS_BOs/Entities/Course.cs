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

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

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

        // Many-to-many relationship with SubjectSpecialty
        public ICollection<SubjectSpecialty> SubjectSpecialties { get; set; }
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
