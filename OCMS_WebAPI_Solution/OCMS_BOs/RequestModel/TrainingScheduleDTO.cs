using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class TrainingScheduleDTO
    {
        public string SubjectID { get; set; }
        public string InstructorID { get; set; }
        public string Location { get; set; }
        public string Room { get; set; }
        public string Notes { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public List<int> DaysOfWeek { get; set; } // e.g., [1, 3, 5] for Monday, Wednesday, Friday
        public TimeOnly ClassTime { get; set; } // e.g., "09:00" for 9:00 AM
    }
}
