using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class CreateCertificateTemplateResponse
    {
        public string CertificateTemplateId { get; set; }
        public string Description { get; set; }
        public string TemplateFile { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string TemplateStatus { get; set; }
        public string TemplateFileWithSas { get; set; }
    }
}
