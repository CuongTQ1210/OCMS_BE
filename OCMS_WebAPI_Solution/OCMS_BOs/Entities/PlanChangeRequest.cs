using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace OCMS_BOs.Entities
{
    public class PlanChangeRequest
    {
        [Key]
        public string PlanRequestId { get; set; }

        [ForeignKey("TrainingPlan")]
        public string PlanId { get; set; }
        public TrainingPlan Plan { get; set; }

        [ForeignKey("RequestUser")]
        public string RequestedBy { get; set; }
        public User RequestedUser { get; set; }

        [Required]
        public string RequestType { get; set; } // edit, delete

        [Required]
        public RequestStatus Status { get; set; } // pending, approved, rejected

        public string ChangeDetails { get; set; }

        [ForeignKey("ApproveUser")]
        public string? ApprovedBy { get; set; } // Nullable, only set if approved
        public User? ApprovedUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
