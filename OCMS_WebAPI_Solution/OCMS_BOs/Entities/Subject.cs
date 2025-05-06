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

        public string SubjectName { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public double PassingScore { get; set; }

        [ForeignKey("CreateByUserId")]
        public string CreateByUserId { get; set; }
        public User? CreateByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Add the many-to-many relationship through the join table
        public List<CourseSubjectSpecialty> CourseSubjectSpecialties { get; set; }

    }
}
