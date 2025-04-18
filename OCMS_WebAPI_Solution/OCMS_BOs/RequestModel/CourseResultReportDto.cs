using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CourseResultReportDto
    {
        public string CourseId { get; set; }
        public string SubjectId { get; set; }
        public int TotalTrainees { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public double AverageScore { get; set; }
    }
}
