using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCMS_BOs.Entities
{
    public class LoginLog
    {
        [Key]
        public string SessionId { get; set; }  // ID phiên, dùng jti từ JWT

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime LoginTime { get; set; }
        public DateTime? SessionExpiry { get; set; }  // Thời gian hết hạn token

        public ICollection<AuditLog> AuditLogs { get; set; }  // Liên kết với AuditLog
    }
}