using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TrainingSchedule
    {
        [Key]
        public string ScheduleID { get; set; }

        [ForeignKey("Subject")]
        public string SubjectID { get; set; }
        public Subject Subject { get; set; }


        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public string Location { get; set; }
        public string Room { get; set; }

        [ForeignKey("InstructorUser")]
        public string InstructorID { get; set; }
        public User Instructor { get; set; }

        [ForeignKey("CreateUser")]
        public string CreatedBy { get; set; }
        public User CreatedByUser { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string Status { get; set; } // scheduled, completed, cancelled
        public string Notes { get; set; }
    }
}
