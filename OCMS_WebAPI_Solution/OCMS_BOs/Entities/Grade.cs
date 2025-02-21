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

        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        // Trainee (Student) Foreign Key
        [ForeignKey("Trainee")]
        public string TraineeId { get; set; }
        public User Trainee { get; set; }

        public int GradeValue { get; set; }
        public DateTime EvaluationDate { get; set; }

        // Instructor Foreign Key
        [ForeignKey("Instructor")]
        public string SubmittedBy { get; set; }
        public User Instructor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
