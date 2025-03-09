using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class CourseResult
    {
        [Key]
        public string ResultID { get; set; }

        [ForeignKey("Course")]
        public string CourseID { get; set; }
        public Course Course { get; set; }

        public DateTime CompletionDate { get; set; }
        public int TotalTrainees { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public double AverageScore { get; set; }

        [ForeignKey("SubmitUser")]
        public string SubmittedBy { get; set; }
        public User SubmittedByUser { get; set; }

        public DateTime SubmissionDate { get; set; }

        [ForeignKey("ApproveUser")]
        public string? ApprovedBy { get; set; }
        public User? ApprovedByUser { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public ResultStatus Status { get; set; } // draft, submitted, approved, rejected
    }
}
