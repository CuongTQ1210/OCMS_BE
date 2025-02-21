using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TraineeNotification
    {
        [Key]
        public int NotificationId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } // Trainee
        public User User { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string NotificationType { get; set; } // Certificate Expiration, Course Enrollment, Grade Update

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
