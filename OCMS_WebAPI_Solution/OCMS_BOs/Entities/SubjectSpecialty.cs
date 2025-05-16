using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class SubjectSpecialty
    {
        [Key]
        public string SubjectSpecialtyId { get; set; }

        [ForeignKey("Specialty")]
        public string SpecialtyId { get; set; }
        public Specialties Specialty { get; set; }

        [ForeignKey("Subject")]
        public string SubjectId { get; set; }
        public Subject Subject { get; set; }
        
        // Many-to-many relationship with Course
        public ICollection<Course> Courses { get; set; }
    }
}
