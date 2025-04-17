using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
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
        Task<ExternalCertificateModel> AddExternalCertificateAsync(string candidateId, ExternalCertificateCreateDTO certificateDto, IFormFile certificateImage, IBlobService blobService, string currentUserId);
        Task<ExternalCertificateModel> UpdateExternalCertificateAsync(
            int externalCertificateId,
            ExternalCertificateUpdateDTO updatedCertificateDto,
            IFormFile certificateImage,
            IBlobService blobService,
            string currentUserId);
        Task<bool> DeleteExternalCertificateAsync(int externalCertificateId, IBlobService blobService);
    }
}
