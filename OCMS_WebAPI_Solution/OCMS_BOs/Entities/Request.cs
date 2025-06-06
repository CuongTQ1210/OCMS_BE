﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Request
    {
        [Key]
        public string RequestId { get; set; }
        [ForeignKey("RequestUser")]
        public string RequestUserId { get; set; }
        public User RequestUser { get; set; }

        public string? RequestEntityId { get; set; }
        public RequestType RequestType { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string Notes { get; set; }

        public RequestStatus Status { get; set; }

        [ForeignKey("ApproveByUser")]
        public string? ApproveByUserId { get; set; } // Nullable, only set if approved
        public User? ApproveBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
