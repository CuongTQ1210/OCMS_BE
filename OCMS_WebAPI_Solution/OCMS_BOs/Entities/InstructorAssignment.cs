using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public   class InstructorAssignment
    {
        [Key]
        public string AssignmentId { get; set; }

        [ForeignKey("CourseSubjectSpecialty")]
        public string CourseSubjectSpecialtyId { get; set; }
        public CourseSubjectSpecialty CourseSubjectSpecialty { get; set; }
        [ForeignKey("InstructorUser")]
        public string InstructorId { get; set; }
        public User Instructor { get; set; }
        [ForeignKey("AssignUser")]
        public string AssignByUserId { get; set; }
        public User AssignByUser { get; set; }
        public DateTime AssignDate { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string Notes { get; set; }

    }
}
