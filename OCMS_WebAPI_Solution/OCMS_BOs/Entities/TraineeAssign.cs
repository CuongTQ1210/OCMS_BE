using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TraineeAssign
    {
        public string TraineeAssignId { get; set; }
        [ForeignKey("User")]
        public string TraineeId { get; set; } //assign trainee to course 
        public User Trainee {  get; set; }
        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }
        
        public DateTime AssignDate { get; set; }= DateTime.Now;
        [ForeignKey("User")]
        public string ApproveByUserId { get; set; }
        public User ApproveByUser { get; set; }
        

    }
}
