using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class TraineeInfoReportDto
    {
        public string TraineeId { get; set; }
        public string TraineeName { get; set; }
        public string Email { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime AssignDate { get; set; }

        public string SubjectId { get; set; }
        public double? TotalGrade { get; set; }
        public string Status { get; set; } // Pass or Fail
    }
}
