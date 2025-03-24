using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class TrainingPlanModel
    {
        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public string Desciption { get; set; }
        public string PlanLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreateByUserId { get; set; }
        public string CreateByUserName { get; set; }
        public string SpecialtyId { get; set; }
        public string SpecialtyName { get; set; }
        public string TrainingPlanStatus { get; set; }

        public List<CourseModel> Courses { get; set; } = new List<CourseModel>();
    }
}
