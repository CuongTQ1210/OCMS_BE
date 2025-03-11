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
        public string DepartmentName { get; set; }
        public string DepartmentDescription { get; set; }
        [ForeignKey("ManagerUser")]
        public string ManagerUserId { get; set; }
        public User Manager { get; set; }
        public string Specialty {  get; set; }
        public DepartmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
