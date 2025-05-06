using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class CourseModel
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string? Description { get; set; }
        public string? CourseRelatedId { get; set; }
        public string CourseLevel { get; set; }
        public string Status { get; set; }
        public string Progress { get; set; }
        public string? ApproveByUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<TraineeAssignModel> Trainees { get; set; } = new List<TraineeAssignModel>();
        public List<SubjectModel> Subjects { get; set; } = new List<SubjectModel>();
        public List<CourseSubjectSpecialtyModel> CourseSubjectSpecialties { get; set; } = new List<CourseSubjectSpecialtyModel>();
        public List<TrainingPlanModel> TrainingPlans { get; set; } = new List<TrainingPlanModel>();
    }
}
