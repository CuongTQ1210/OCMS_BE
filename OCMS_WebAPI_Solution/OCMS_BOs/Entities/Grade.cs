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

        [ForeignKey("TraineeAssign")]
        public string TraineeAssignID { get; set; }
        public TraineeAssign TraineeAssign { get; set; }

        [ForeignKey("Subject")]
        public string SubjectId { get; set; }
        public Subject Subject { get; set; }
        
        public double Score { get; set; }
        public GradeStatus gradeStatus { get; set; }
        public string Remarks { get; set; }

        
        [ForeignKey("GradeUser")]
        public string GradedByInstructorId { get; set; }
        public User GradedByInstructor { get; set; }

        public DateTime EvaluationDate { get; set; }= DateTime.Now;

        public DateTime UpdateDate { get; set; }=DateTime.Now;
       
    }
}
