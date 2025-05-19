using System;
using System.Collections.Generic;

namespace OCMS_BOs.ViewModel
{
    public class SubjectSpecialtyModel
    {
        public string SubjectSpecialtyId { get; set; }
        public string SpecialtyId { get; set; }
        public string SubjectId { get; set; }
        
        // Navigation properties
        public SpecialtyModel Specialty { get; set; }
        public SubjectModel Subject { get; set; }
    }
} 