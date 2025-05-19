using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Class
    {
        [Key]
        public string ClassId { get; set; }
        public string ClassName { get; set; }
    }
}
