using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class SubjectModel
    {
        public string SubjectId { get; set; }
        public string CourseId { get; set; }
        public string SubjectName { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public double PassingScore { get; set; }
        public string CreateByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<InstructorAssignmentModel> Instructors { get; set; } = new List<InstructorAssignmentModel>();
        public List<TrainingScheduleModel> trainingSchedules { get; set; }= new List<TrainingScheduleModel>();
    }
}
