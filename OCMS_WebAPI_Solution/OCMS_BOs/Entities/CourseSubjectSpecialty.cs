using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class CourseSubjectSpecialty
    {
        [Key]
        public string Id { get; set; }

        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey("Subject")]
        public string SubjectId { get; set; }
        public Subject Subject { get; set; }

        [ForeignKey("Specialty")]
        public string SpecialtyId { get; set; }
        public Specialties Specialty { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CreatedByUser")]
        public string CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public List<TraineeAssign> Trainees { get; set; }
        public List<InstructorAssignment> Instructors { get; set; }
        public List<TrainingSchedule> Schedules { get; set; }
        public string Notes { get; set; }
    }
}
