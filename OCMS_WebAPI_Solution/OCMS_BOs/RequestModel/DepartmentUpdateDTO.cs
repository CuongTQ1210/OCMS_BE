using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class DepartmentUpdateDTO
    {
        public string DepartmentName { get; set; }
        public string DepartmentDescription { get; set; }
        public string ManagerId { get; set; }
    }
}
