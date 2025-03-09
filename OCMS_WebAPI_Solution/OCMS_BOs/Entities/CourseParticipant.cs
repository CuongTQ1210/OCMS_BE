using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class CourseParticipant
    {
        [Key]
        public int ParticipantId { get; set; }

        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Role { get; set; } // Trainee, Instructor

        [Required]
        public CourseParticipantStatus Status { get; set; } // active, withdrawn

        [ForeignKey("Grade")]
        public string? GradeId { get; set; } // Nullable, for Trainees only
        public Grade Grade { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
