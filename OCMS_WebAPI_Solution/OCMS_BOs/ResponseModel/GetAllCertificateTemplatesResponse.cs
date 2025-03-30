using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class GetAllCertificateTemplatesResponse
    {
        public List<CertificateTemplateItem> Templates { get; set; }

        public class CertificateTemplateItem
        {
            public string CertificateTemplateId { get; set; }
            public string TemplateName { get; set; }
            public string Description { get; set; }
            public string TemplateStatus { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
