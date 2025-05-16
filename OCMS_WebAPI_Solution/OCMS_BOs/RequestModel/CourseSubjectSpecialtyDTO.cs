using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    // This DTO can be used to add a subject-specialty to a course
    public class CourseSubjectSpecialtyDTO
    {
        public string CourseId { get; set; }
        public string SubjectId { get; set; }
        public string SpecialtyId { get; set; }
    }

    // This DTO can be used when adding multiple subject-specialties to a course at once
    public class BulkCourseSubjectSpecialtyDTO
    {
        public string CourseId { get; set; }
        public List<SubjectSpecialtyRef> SubjectSpecialties { get; set; } = new List<SubjectSpecialtyRef>();
    }

    public class SubjectSpecialtyRef
    {
        public string SubjectId { get; set; }
        public string SpecialtyId { get; set; }
    }
}
