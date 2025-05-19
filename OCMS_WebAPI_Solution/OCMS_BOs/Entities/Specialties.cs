using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Specialties
    {
        [Key]
        public string SpecialtyId { get; set; }

        [Required]
        public string SpecialtyName { get; set; }

        public string Description { get; set; }

        [ForeignKey("ParentSpecialty")]
        public string? ParentSpecialtyId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CreatedByUser")]
        public string? CreatedByUserId { get; set; }

        [ForeignKey("UpdatedByUser")]
        public string? UpdatedByUserId { get; set; }

        public int Status { get; set; } = 1;

        // Navigation properties
        public virtual Specialties ParentSpecialty { get; set; }
        public virtual ICollection<Specialties> SubSpecialties { get; set; }
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
    }
}
