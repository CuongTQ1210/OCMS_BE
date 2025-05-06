using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class DeleteAllSubjectInCourseSpecialty
    {
        [Required(ErrorMessage = "CourseId is required")] 
        public string CourseId { get; set; }
        [Required(ErrorMessage = "CourseId is required")]
        public string SpecialtyId { get; set; }

    }
}
