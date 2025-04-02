using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class UpdateSpecialtyDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "SpecialtyName must contain 2 to 100 words.")]
        public string SpecialtyName { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Description must contain 2 to 255 words.")]
        public string Description { get; set; }
        public string? ParentSpecialtyId { get; set; }
        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(SpecialtyStatus), ErrorMessage = "Status must be Active or Inactive.")]
        public SpecialtyStatus Status { get; set; }
    }
}
