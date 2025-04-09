using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class GradeDTO
    {
        public string TraineeAssignID { get; set; }
        public string SubjectId { get; set; }

        public double ParticipantScore { get; set; }
        public double AssignmentScore { get; set; }
        public double FinalExamScore { get; set; }
        public double? FinalResitScore { get; set; }

        public string Remarks { get; set; }
    }
}
