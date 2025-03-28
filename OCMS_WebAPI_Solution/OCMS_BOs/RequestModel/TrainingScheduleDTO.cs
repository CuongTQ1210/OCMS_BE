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
        public string Location { get; set; }
        public string Room { get; set; }
        public string CreatedBy { get; set; }
        public ScheduleStatus Status { get; set; }
        public string Notes { get; set; }
    }
}
