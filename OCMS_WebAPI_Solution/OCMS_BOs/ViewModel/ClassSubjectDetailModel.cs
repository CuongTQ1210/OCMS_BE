using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class ClassSubjectDetailModel
    {
        public string ClassSubjectId { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public double PassingScore { get; set; }
        public string SpecialtyId { get; set; }
        public string SpecialtyName { get; set; }
        public string InstructorAssignmentID { get; set; }
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
        public string InstructorEmail { get; set; }
        public List<TrainingScheduleModel> Schedules { get; set; }
        public List<TraineeAssignModel> TraineeAssignments { get; set; }
        public int EnrolledTraineesCount { get; private set; } // Changed to private setter

        public void SetEnrolledTraineesCount(int count) // Added a method to set the value
        {
            EnrolledTraineesCount = count;
        }
    }
}
