using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IExternalCertificateService
    {
        Task<IEnumerable<ExternalCertificateModel>> GetExternalCertificatesByCandidateIdAsync(string candidateId);
        Task<ExternalCertificateModel> AddExternalCertificateAsync(string candidateId, ExternalCertificateModel certificateDto, IFormFile certificateImage, IBlobService blobService, string currentUserId);
        Task<ExternalCertificateModel> UpdateExternalCertificateAsync(int externalCertificateId, ExternalCertificateModel updatedCertificateDto, IBlobService blobService);
        Task<bool> DeleteExternalCertificateAsync(int externalCertificateId, IBlobService blobService);
    }
}
