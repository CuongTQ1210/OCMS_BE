using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class TrainingScheduleModel
    {
        public string ScheduleID { get; set; }
        public string SubjectID { get; set; }
        public string SubjectName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Location { get; set; }
        public string Room { get; set; }
        public string InstructorID { get; set; }
        public string InstructorName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Status { get; set; } // incoming, completed, cancelled
        public string Notes { get; set; }

        public string DaysOfWeek { get; set; } // e.g., "Monday, Wednesday, Friday"

        public TimeOnly ClassTime { get; set; } // e.g., "09:00"
    }
}
