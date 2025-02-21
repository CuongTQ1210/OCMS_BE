using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class BackupLog
    {
        [Key]
        public int BackupId { get; set; }

        [Required]
        public string BackupFilePath { get; set; } // Path of backup file

        public DateTime BackupTimestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string CreatedBy { get; set; }
        public User User { get; set; }
    }
}
