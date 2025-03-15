using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Department
    {
        [Key]
        public string DepartmentId {  get; set; }
        [Required]
        public string DepartmentName { get; set; }
        public string DepartmentDescription { get; set; }
        [ForeignKey("Specialties")]
        public string SpecialtyId { get; set; }
        [ForeignKey("ManagerUser")]
        public string ManagerUserId { get; set; }
        public DepartmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User Manager { get; set; }
        public virtual Specialties Specialty { get; set; }
    }
}
