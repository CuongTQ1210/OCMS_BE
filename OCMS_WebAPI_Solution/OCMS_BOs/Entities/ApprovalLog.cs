using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class ApprovalLog
    {
        [Key]
        public int ApprovalId { get; set; }

        [Required]
        public string ActionType { get; set; } // course_create, course_update, course_delete

        [ForeignKey("RequestUser")]
        public string RequestedBy { get; set; } // Education Officer
        public User RequestedUser { get; set; }

        [ForeignKey("ApproveUser")]
        public string? ApprovedBy { get; set; } // Director
        public User? ApprovedUser { get; set; }

        [Required]
        public RequestStatus Status { get; set; } // pending, approved, rejected

        public string ActionDetails { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
