using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public CourseStatus Status { get; set; }
        public Progress Progress { get; set; }
    }

    public class CreateCourseDTO
    {
        [Required(ErrorMessage = "Course ID is required.")]
        public string CourseId { get; set; }

        [Required(ErrorMessage = "Training Plan ID is required.")]
        public string TrainingPlanId { get; set; }

        [Required(ErrorMessage = "Course Name is required.")]
        [StringLength(100, ErrorMessage = "Course Name must not exceed 100 characters.")]
        public string CourseName { get; set; }

        [Required(ErrorMessage = "Course Level is required.")]
        [EnumDataType(typeof(CourseLevel), ErrorMessage = "Invalid Course Level.")]
        public CourseLevel CourseLevel { get; set; }

    }
}
