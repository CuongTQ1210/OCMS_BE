using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OCMS_BOs.RequestModel
{
    public class CourseDTO
    {
        public string CourseId { get; set; }
        public string TrainingPlanId { get; set; }
        public string CourseName { get; set; }
        public CourseLevel CourseLevel { get; set; }
        public CourseStatus Status { get; set; }
        public Progress Progress { get; set; }
    }

    public class CourseCreateDTO
    {
        [Required(ErrorMessage = "TrainingPlanId is required.")]
        public string TrainingPlanId { get; set; }

        [Required(ErrorMessage = "CourseName is required.")]
        public string CourseName { get; set; }

        [Required(ErrorMessage = "CourseLevel is required.")]
        [EnumDataType(typeof(CourseLevel), ErrorMessage = "Invalid course level.")]
        public CourseLevel CourseLevel { get; set; }
    }
}
