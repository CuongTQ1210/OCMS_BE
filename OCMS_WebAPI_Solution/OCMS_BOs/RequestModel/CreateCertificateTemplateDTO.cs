using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CreateCertificateTemplateDTO
    {
        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Description { get; set; }

        [Required]
        public IFormFile HtmlTemplate { get; set; }
    }
}
