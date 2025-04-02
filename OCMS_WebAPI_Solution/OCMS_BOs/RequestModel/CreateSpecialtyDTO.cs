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
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "SpecialtyName must contain 2 to 100 words.")]
        public string SpecialtyName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Description must contain 2 to 255 words.")]
        public string Description { get; set; } = string.Empty;

        public string? ParentSpecialtyId { get; set; }

    }
}
