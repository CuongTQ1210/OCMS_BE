using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CourseParticipantDto
    {
        public string CourseId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; } // Trainee, Instructor
        public string Status { get; set; } // Active, Withdrawn
        public string? GradeId { get; set; } // Optional field
    }
}
