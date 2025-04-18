using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class GradeModel
    {
        public string GradeId { get; set; }
        public string TraineeAssignID { get; set; }
        public string SubjectId { get; set; }

        public double ParticipantScore { get; set; }
        public double AssignmentScore { get; set; }
        public double FinalExamScore { get; set; }
        public double? FinalResitScore { get; set; }

        public double TotalScore { get; set; }
        public string GradeStatus { get; set; }
        public string Remarks { get; set; }

        public string GradedByInstructorId { get; set; }

        public DateTime EvaluationDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public string Fullname { get; set; }
    }
}
