using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CourseUpdateDTO
    { 
        public string Description { get; set; }
        public string CourseName { get; set; }
        public string? CourseRelatedId { get; set; }
    }
}
