using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CourseSubjectSpecialtyDTO
    {
        public string CourseId { get; set; }
        public string SubjectId { get; set; }
        public string SpecialtyId { get; set; }
        public string CreatedByUserId { get; set; }
        public string? Notes { get; set; }
    }
}
