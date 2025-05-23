using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class ClassSubjectDTO
    {
        [Required]
        public string ClassId { get; set; }

        [Required]
        public string SubjectSpecialtyId { get; set; }

        [Required]
        public string InstructorAssignmentID { get; set; }

        public string Notes { get; set; }
    }
}
