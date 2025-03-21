using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class SubjectDTO
    {
        public string CourseId { get; set; }
        public string SubjectName { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public double PassingScore { get; set; }
    }
}
