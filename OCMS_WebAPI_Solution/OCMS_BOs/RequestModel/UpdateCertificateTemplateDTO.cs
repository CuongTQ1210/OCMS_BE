using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class UpdateCertificateTemplateDTO
    {
        public string Description { get; set; }
        public TemplateStatus? TemplateStatus { get; set; }
        public IFormFile? HtmlTemplate { get; set; }
    }
}
