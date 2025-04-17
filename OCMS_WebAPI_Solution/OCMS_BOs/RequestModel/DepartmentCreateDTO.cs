using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class DepartmentCreateDTO
    {
        [Required]
        public string DepartmentName { get; set; }
        public string DepartmentDescription { get; set; }
        [Required]
        public string SpecialtyId { get; set; }
        
        public string ManagerId { get; set; }

    }
}
