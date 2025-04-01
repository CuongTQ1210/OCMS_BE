using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CreateSpecialtyDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string SpecialtyName { get; set; }

        public string Description { get; set; }

        public string? ParentSpecialtyId { get; set; }

        public int Status { get; set; } = 1;
    }
}
