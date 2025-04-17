using AutoMapper;
using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class ExternalCertificateService : IExternalCertificateService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;
        private readonly IMapper _mapper;
        private readonly IExternalCertificateRepository _externalCertificateRepository;
        public ExternalCertificateService(UnitOfWork unitOfWork, IBlobService blobService, IMapper mapper, IExternalCertificateRepository externalCertificateRepository)
        {
            _unitOfWork = unitOfWork;
            _blobService = blobService;
            _mapper = mapper;
            _externalCertificateRepository = externalCertificateRepository;
        }

        #region Add External Certificate
        public async Task<ExternalCertificateModel> AddExternalCertificateAsync(string candidateId, ExternalCertificateCreateDTO certificateDto, IFormFile certificateImage, IBlobService blobService, string currentUserId)
        {
            var certificate = _mapper.Map<ExternalCertificate>(certificateDto);
            await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Kiểm tra candidate có tồn tại không
                    var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(candidateId);
                    if (candidate == null)
                    {
                        throw new KeyNotFoundException($"Candidate with ID {candidateId} not found");
                    }

                    // Kiểm tra các trường bắt buộc
                    if (string.IsNullOrEmpty(certificateDto.CertificateCode) ||
                        string.IsNullOrEmpty(certificateDto.CertificateName) ||
                        string.IsNullOrEmpty(certificateDto.IssuingOrganization) ||
                        string.IsNullOrEmpty(certificateDto.CandidateId) ||
                        certificateImage == null)
                    {
                        throw new ArgumentException("CertificateCode, CertificateName, CertificateProvider, CandidateId, and CertificateImage are required.");
                    }

                    

                    // Gán các thuộc tính đặc biệt
                    certificate.CandidateId = certificateDto.CandidateId;
                    certificate.CreatedAt = DateTime.Now;
                    certificate.VerifyDate = DateTime.Now;
                    certificate.IssuingOrganization = certificateDto.IssuingOrganization; // Gán CertificateProvider vào IssuingOrganization

                    
                    certificate.VerifyByUserId = currentUserId; 

                    // Xử lý hình ảnh (bắt buộc)
                    string blobName = $"{candidateId}_{certificateDto.CertificateCode}_{DateTime.Now.Ticks}.jpg";
                    using (var stream = certificateImage.OpenReadStream())
                    {
                        var fileUrl = await blobService.UploadFileAsync("externalcertificates", blobName, stream, "image/jpeg");
                        // Lưu URL gốc (không có SAS token)
                        certificate.CertificateFileURL = blobService.GetBlobUrlWithoutSasToken(fileUrl);
                    }

                    // Thêm vào repository và lưu
                    await _unitOfWork.ExternalCertificateRepository.AddAsync(certificate);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Error adding external certificate", ex);
                }
            });

            var result = _mapper.Map<ExternalCertificateModel>(certificate);

            // Thêm SAS token cho URL trong response
            if (!string.IsNullOrEmpty(certificate.CertificateFileURL))
            {
                result.CertificateFileURLWithSas = await blobService.GetBlobUrlWithSasTokenAsync(
                    certificate.CertificateFileURL, TimeSpan.FromHours(1));
            }

            return result;
        }
        #endregion

        #region Delete External Certificate
        public async Task<bool> DeleteExternalCertificateAsync(int externalCertificateId, IBlobService blobService)
        {
            bool result = false;

            await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                // Bắt đầu transaction để đảm bảo tính nhất quán
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Lấy thông tin ExternalCertificate từ cơ sở dữ liệu dựa trên ID
                    var certificate = (await _unitOfWork.ExternalCertificateRepository.FindAsync(c => c.ExternalCertificateId == externalCertificateId)).FirstOrDefault();
                    if (certificate == null)
                    {
                        throw new KeyNotFoundException($"ExternalCertificate với ID {externalCertificateId} không tồn tại");
                    }

                    // Lưu URL của hình ảnh để xóa sau (nếu có)
                    string certificateFileURL = certificate.CertificateFileURL;

                    // Xóa bản ghi ExternalCertificate khỏi cơ sở dữ liệu
                    await _externalCertificateRepository.RemoveAsync(externalCertificateId);
                    await _unitOfWork.SaveChangesAsync();

                    // Xóa hình ảnh khỏi Blob storage nếu tồn tại
                    if (!string.IsNullOrEmpty(certificateFileURL))
                    {
                        await blobService.DeleteFileAsync(certificateFileURL);
                    }

                    // Commit transaction nếu mọi thứ thành công
                    await _unitOfWork.CommitTransactionAsync();
                    result = true;
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi xảy ra
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Lỗi khi xóa ExternalCertificate", ex);
                }
            });

            return result;
        }
        #endregion

        #region Get External Certificates By Candidate Id
        public async Task<IEnumerable<ExternalCertificateModel>> GetExternalCertificatesByCandidateIdAsync(string candidateId)
        {
            var certificates = await _unitOfWork.ExternalCertificateRepository.FindAsync(c => c.CandidateId == candidateId);
            var certificateModels = _mapper.Map<IEnumerable<ExternalCertificateModel>>(certificates);
            foreach (var cert in certificateModels)
            {
                if (!string.IsNullOrEmpty(cert.CertificateFileURL))
                {
                    cert.CertificateFileURLWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(
                        cert.CertificateFileURL, TimeSpan.FromHours(1));
                }
            }
            return certificateModels;
        }
        #endregion

        #region Update External Certificate
        public async Task<ExternalCertificateModel> UpdateExternalCertificateAsync(
            int externalCertificateId,
            ExternalCertificateUpdateDTO updatedCertificateDto,
            IFormFile certificateImage,
            IBlobService blobService,
            string currentUserId)
        {
            var existingCertificate = (await _unitOfWork.ExternalCertificateRepository
                        .FindAsync(c => c.ExternalCertificateId == externalCertificateId))
                        .FirstOrDefault();
            if (existingCertificate == null)
                throw new KeyNotFoundException($"ExternalCertificate with ID {externalCertificateId} not found");

            await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Validate required fields
                    if (string.IsNullOrEmpty(updatedCertificateDto.CertificateCode) ||
                        string.IsNullOrEmpty(updatedCertificateDto.CertificateName) ||
                        string.IsNullOrEmpty(updatedCertificateDto.IssuingOrganization))
                    {
                        throw new ArgumentException("CertificateCode, CertificateName, CertificateProvider are required.");
                    }

                    string oldCertificateFileURL = existingCertificate.CertificateFileURL;

                    // Map updated DTO to entity
                    _mapper.Map(updatedCertificateDto, existingCertificate);

                    // Manually update special fields
                    existingCertificate.VerifyDate = DateTime.Now;
                    existingCertificate.IssuingOrganization = updatedCertificateDto.IssuingOrganization;
                    existingCertificate.VerifyByUserId = currentUserId;

                    // Handle image replacement if new image provided
                    if (certificateImage != null)
                    {
                        string blobName = $"{existingCertificate.CandidateId}_{updatedCertificateDto.CertificateCode}_{DateTime.Now.Ticks}.jpg";
                        using (var stream = certificateImage.OpenReadStream())
                        {
                            var fileUrl = await blobService.UploadFileAsync("externalcertificates", blobName, stream, "image/jpeg");
                            existingCertificate.CertificateFileURL = blobService.GetBlobUrlWithoutSasToken(fileUrl);
                        }
                    }

                    await _unitOfWork.ExternalCertificateRepository.UpdateAsync(existingCertificate);
                    await _unitOfWork.SaveChangesAsync();

                    // Delete old image if new one was uploaded
                    if (certificateImage != null && !string.IsNullOrEmpty(oldCertificateFileURL))
                    {
                        await blobService.DeleteFileAsync(oldCertificateFileURL);
                    }

                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Error updating external certificate", ex);
                }
            });

            var result = _mapper.Map<ExternalCertificateModel>(existingCertificate);

            // Add SAS token to the file URL for response
            if (!string.IsNullOrEmpty(existingCertificate.CertificateFileURL))
            {
                result.CertificateFileURLWithSas = await blobService.GetBlobUrlWithSasTokenAsync(
                    existingCertificate.CertificateFileURL, TimeSpan.FromHours(1));
            }

            return result;
        }
        #endregion
    }
}