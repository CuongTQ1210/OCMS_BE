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
        [ForeignKey("ClassSubject")]
        public string ClassSubjectId { get; set; }
        public ClassSubject ClassSubject { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public TimeSpan SubjectPeriod { get; set; }

        public TimeOnly ClassTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public string Location { get; set; }
        public string Room { get; set; }

        [ForeignKey("CreateByUser")]
        public string CreatedByUserId { get; set; }
        public User CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ScheduleStatus Status { get; set; } 
        public string Notes { get; set; }

    }
}
