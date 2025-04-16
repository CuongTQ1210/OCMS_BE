using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class UpdateDecisionTemplateDTO
    {
        public string TemplateName { get; set; }

        public IFormFile TemplateContent { get; set; }

        public string Description { get; set; }
    }
}
