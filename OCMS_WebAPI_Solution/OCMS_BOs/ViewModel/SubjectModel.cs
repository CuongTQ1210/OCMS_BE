using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class SubjectModel
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string? Description { get; set; }
        public int Credits { get; set; }
        public double PassingScore { get; set; }
        public string CreateByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // List of courses that use this subject
        public List<CourseModel> Courses { get; set; } = new List<CourseModel>();
    }
}
