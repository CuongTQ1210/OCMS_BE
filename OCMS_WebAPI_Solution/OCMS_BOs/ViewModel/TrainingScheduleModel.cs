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
        public string ClassSubjectId { get; set; }
        public string SubjectId {  get; set; }
        public string SubjectName { get; set; }
        public string InstructorID { get; set; }
        public string InstructorName { get; set; }
        public string DaysOfWeek { get; set; } // e.g., "Monday, Wednesday"
        public TimeSpan SubjectPeriod { get; set; }
        public TimeOnly ClassTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Location { get; set; }
        public string Room { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
}
