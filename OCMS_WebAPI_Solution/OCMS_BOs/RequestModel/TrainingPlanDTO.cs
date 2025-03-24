using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class TrainingPlanDTO
    {
        public string PlanName { get; set; }
        public string Desciption { get; set; }
        public PlanLevel PlanLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SpecialtyId { get; set; }
    }
}
