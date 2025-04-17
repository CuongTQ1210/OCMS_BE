using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class ExternalCertificateModel
    {
        public string Id { get; set; }
        public string CertificateCode { get; set; }
        public string CertificateName { get; set; }
        public string CertificateProvider { get; set; }
        public string CertificateFileURL { get; set; }
        public string CandidateId { get; set; }
        public string CertificateFileURLWithSas { get; set; }

    }
}
