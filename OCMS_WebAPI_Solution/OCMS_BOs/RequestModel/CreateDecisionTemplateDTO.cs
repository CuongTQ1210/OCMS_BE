using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CreateDecisionTemplateDTO
    {
        public string templateName { get; set; }
        public IFormFile templateContent { get; set; }
        public string description { get; set; }
    }
}
