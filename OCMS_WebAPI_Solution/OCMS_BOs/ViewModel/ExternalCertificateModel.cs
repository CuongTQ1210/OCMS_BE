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
        [Required(ErrorMessage = "CertificateCode is required")]
        public string CertificateCode { get; set; }
        [Required(ErrorMessage = "CertificateName is required")]
        public string CertificateName { get; set; }
        [Required(ErrorMessage = "CertificateProvider is required")]
        public string CertificateProvider { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CertificateFileURL { get; set; }
        [Required(ErrorMessage = "CandidateId is required")]
        public string CandidateId { get; set; }
        [Required(ErrorMessage = "CertificateImage is required")]
        public IFormFile CertificateImage { get; set; }
        public string CertificateFileURLWithSas { get; set; }

    }
}
