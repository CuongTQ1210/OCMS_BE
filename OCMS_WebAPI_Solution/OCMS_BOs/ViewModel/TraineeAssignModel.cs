using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class TraineeAssignModel
    {
        public string TraineeAssignId { get; set; }
        public string TraineeId { get; set; }
        public string CourseId { get; set; }
        public string Notes { get; set; }

        public string RequestStatus { get; set; }
        public DateTime AssignDate { get; set; }
        public string? ApproveByUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }

        public string RequestId { get; set; }
    }
}
