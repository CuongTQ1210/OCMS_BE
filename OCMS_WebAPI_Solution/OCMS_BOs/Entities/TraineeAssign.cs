using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TraineeAssign
    {
        [Key]
        public string TraineeAssignId { get; set; }
        [ForeignKey("TraineeUser")]
        public string TraineeId { get; set; } //assign trainee to course 
        public User Trainee {  get; set; }
        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }
        
        public DateTime AssignDate { get; set; }= DateTime.Now;
        [ForeignKey("ApproveUser")]
        public string ApproveByUserId { get; set; }
        public User ApproveByUser { get; set; }
        public DateTime ApprovalDate {  get; set; }= DateTime.Now;

        public string Notes;

    }
}
