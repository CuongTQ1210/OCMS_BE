using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class InstructorAssignmentModel
    {
        public string AssignmentId { get; set; }
        public string SubjectId { get; set; }
        public string InstructorId { get; set; }
        public string AssignByUserId { get; set; }
        public DateTime AssignDate { get; set; }
        public string RequestStatus { get; set; } 
        public string Notes { get; set; }
    }
}
