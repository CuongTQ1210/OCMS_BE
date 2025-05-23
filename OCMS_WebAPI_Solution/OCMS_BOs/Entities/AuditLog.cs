using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCMS_BOs.Entities
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("LoginLog")]
        public string SessionId { get; set; }
        public LoginLog LoginLog { get; set; }

        public string Action { get; set; }
        public string ActionDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}