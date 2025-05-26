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
        public string TraineeId { get; set; }
        public string Fullname { get; set; }
        public string CourseId { get; set; }
        public string TraineeAssignId { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
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
    }
}
