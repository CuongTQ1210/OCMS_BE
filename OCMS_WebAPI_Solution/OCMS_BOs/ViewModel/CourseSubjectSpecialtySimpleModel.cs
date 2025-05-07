using System;

namespace OCMS_BOs.ViewModel
{
    public class CourseSubjectSpecialtySimpleModel
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SpecialtyId { get; set; }
        public string SpecialtyName { get; set; }
        public string Description { get; set; }
    }
}