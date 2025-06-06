﻿using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ICertificateService
    {
        Task<List<CertificateModel>> AutoGenerateCertificatesForPassedTraineesAsync(string courseId, string issuedByUserId);
        Task<CertificateModel> GetCertificateByIdAsync(string certificateId);
        Task<List<CertificateModel>> GetPendingCertificatesWithSasUrlAsync();
        Task<List<CertificateModel>> GetCertificatesByUserIdWithSasUrlAsync(string userId);
        Task<List<CertificateModel>> GetActiveCertificatesWithSasUrlAsync();
        Task<CertificateModel> CreateCertificateManuallyAsync(string userId, string courseId, string issuedByUserId);
        Task<(bool success, string message)> RevokeCertificateAsync(string certificateId, RevokeCertificateDTO dto);
        Task<List<CertificateModel>> GetRevokedCertificatesWithSasUrlAsync();
        Task<CertificateRenewalHistoryModel> GetCertificateRenewalHistoryAsync(string certificateId);
        Task<List<CertificateRenewalHistoryModel>> GetUserCertificateRenewalHistoryAsync(string userId);
        //Task<CertificateModel> CreateCertificateWithCustomGradesAsync(string courseId, string userId, string issuedByUserId, List<Grade> customGrades);
    }
}
