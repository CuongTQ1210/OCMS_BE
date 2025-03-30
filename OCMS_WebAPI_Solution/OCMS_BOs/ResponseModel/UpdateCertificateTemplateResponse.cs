using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class UpdateCertificateTemplateResponse
    {
        public string CertificateTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string TemplateFile { get; set; }
        public TemplateStatus TemplateStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public string ApprovedByUserId { get; set; }

        // Thông tin người tạo và phê duyệt (nếu mapping bao gồm cả thông tin này)
        public string CreatedByUserName { get; set; }
        public string ApprovedByUserName { get; set; }
    }
}
