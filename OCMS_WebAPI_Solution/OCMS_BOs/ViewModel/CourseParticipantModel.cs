using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class CourseParticipantModel
    {
        public string ParticipantId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; } // Display Course Name
        public string UserId { get; set; }
        public string UserName { get; set; } // Display User Name
        public string Role { get; set; } // Trainee, Instructor
        public string Status { get; set; } // Active, Withdrawn
        public string? GradeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
