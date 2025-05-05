using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class CourseSubjectSpecialtyModel
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string SubjectId { get; set; }
        public string SpecialtyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public string? Notes { get; set; }

        public CourseModel Course { get; set; }
        public SubjectModel Subject { get; set; }
        public SpecialtyModel Specialty { get; set; }
        public List<TraineeAssignModel> Trainees { get; set; } = new List<TraineeAssignModel>();
        public List<InstructorAssignmentModel> Instructors { get; set; } = new List<InstructorAssignmentModel>();
        public List<TrainingScheduleModel> Schedules { get; set; } = new List<TrainingScheduleModel>();
    }
}
