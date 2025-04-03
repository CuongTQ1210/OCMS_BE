using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CourseDTO
    {
        public string CourseId { get; set; }
        public string TrainingPlanId { get; set; }
        public string CourseName { get; set; }
        public CourseLevel CourseLevel { get; set; }
    }
}
