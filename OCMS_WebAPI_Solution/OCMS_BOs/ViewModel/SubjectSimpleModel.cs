using System;

namespace OCMS_BOs.ViewModel
{
    public class SubjectSimpleModel
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public double PassingScore { get; set; }
    }
}