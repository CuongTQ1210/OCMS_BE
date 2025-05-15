using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class ClassSubject
    {
        [Key]
        public string ClassSubjectId { get; set; }

        [ForeignKey("Class")]
        public string ClassId { get; set; }
        public Class Class { get; set; }

        [ForeignKey("Subject")]
        public string SubjectId { get; set; }
        public Subject Subject { get; set; }


        [ForeignKey("InstructorAssignment")]
        public string InstructorAssignmentID { get; set; }
        public InstructorAssignment InstructorAssignment { get; set; }

        public ICollection<TraineeAssign> traineeAssigns { get; set; }
        public TrainingSchedule Schedule { get; set; }
    }
}
