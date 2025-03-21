using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Subject
    {
        [Key]
        public string SubjectId { get; set; }

        [ForeignKey("Course")]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        public string SubjectName { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public double PassingScore { get; set; }
        public string Schedule { get; set; }
        [ForeignKey("CreateUser")]
        public string CreateByUserId { get; set; }
        public User CreateByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<InstructorAssignment> Instructors { get; set; }
        public List<TrainingSchedule> Schedules { get; set; }
    }
}
