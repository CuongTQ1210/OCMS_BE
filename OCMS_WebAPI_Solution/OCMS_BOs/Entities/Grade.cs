using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Grade
    {
        [Key]
        public string GradeId { get; set; }

        [ForeignKey("Subject")]
        public string SubjectId { get; set; }
        public Subject Subject { get; set; }
        
        
        public string TraineeId { get; set; }
        public User Trainee { get; set; }

        public int GradeValue { get; set; }
        public DateTime EvaluationDate { get; set; }

        public string SubmittedBy { get; set; }
        public User Instructor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
