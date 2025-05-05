using AutoMapper;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OCMS_Services.IService;
using OCMS_Repositories.IRepository;
using System.Net;

namespace OCMS_Services.Service
{
    public class CertificateService : ICertificateService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ITraineeAssignRepository _traineeAssignRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CertificateService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;

        // Cache keys
        private const string TEMPLATE_HTML_CACHE_KEY = "template_html_{0}";
        private const int TEMPLATE_CACHE_MINUTES = 60;

        public CertificateService(
            UnitOfWork unitOfWork,
            IBlobService blobService,
            INotificationService notificationService,
            IUserRepository userRepository,
            ITraineeAssignRepository traineeAssignRepository,
            IGradeRepository gradeRepository,
            ICourseRepository courseRepository,
            ILogger<CertificateService> logger,
            IMemoryCache memoryCache,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _traineeAssignRepository = traineeAssignRepository ?? throw new ArgumentNullException(nameof(traineeAssignRepository));
            _gradeRepository = gradeRepository ?? throw new ArgumentNullException(nameof(gradeRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        #region Auto Generate Certificates for Passed Trainees
        /// <summary>
        /// Automatically generates certificates for all trainees who have passed all subjects in a course
        /// and sends notifications to the HeadMaster for digital signature approval
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <param name="issuedByUserId">The ID of the user issuing the certificates (typically a training staff)</param>
        /// <returns>A list of created certificate models</returns>
        public async Task<List<CertificateModel>> AutoGenerateCertificatesForPassedTraineesAsync(string courseId, string issuedByUserId)
        {
            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(issuedByUserId))
                throw new ArgumentException("CourseId and IssuedByUserId are required");

            var createdCertificates = new List<CertificateModel>();

            try
            {
                // 1. Get course data efficiently
                var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                if (course == null || !course.CourseSubjectSpecialties.Any() || course.Status != CourseStatus.Approved)
                {
                    throw new Exception($"Course with ID {courseId} not found or not active or has no subjects!");
                }

                int subjectCount = course.CourseSubjectSpecialties.Count;

                // 2. Get template data with caching
                var templateId = await GetTemplateIdByCourseLevelAsync(course.CourseLevel);
                if (string.IsNullOrEmpty(templateId))
                {
                    throw new Exception($"No active template for course level {course.CourseLevel}");
                }

                var certificateTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(templateId);
                if (certificateTemplate == null || certificateTemplate.templateStatus != TemplateStatus.Active)
                {
                    _logger.LogWarning($"Template with ID {templateId} not found or not active");
                    throw new Exception($"Certificate template with ID {templateId} not found or not active");
                }

                var templateHtml = await GetCachedTemplateHtmlAsync(certificateTemplate.TemplateFile);
                var templateType = GetTemplateTypeFromName(certificateTemplate.TemplateName);

                // 3. Get all data needed for certificate generation in bulk
                var traineeAssignments = await _traineeAssignRepository.GetTraineeAssignmentsByCourseIdAsync(courseId);
                var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.CourseId == courseId);
                var allGrades = await _gradeRepository.GetGradesByCourseIdAsync(courseId);

                // 4. Process data
                var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
                var traineeIds = traineeAssignments.Select(ta => ta.TraineeId).Distinct().ToList();

                _logger.LogInformation($"Found {traineeIds.Count()} trainees enrolled in course, {traineeWithCerts.Count} already have certificates");

                // 5. Get trainee data efficiently
                var trainees = await _userRepository.GetUsersByIdsAsync(traineeIds);
                var traineeDict = trainees.ToDictionary(t => t.UserId);

                // 6. Process grades
                var gradesByTraineeAssign = allGrades
                    .GroupBy(g => g.TraineeAssignID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var issueDate = DateTime.Now;

                // 7. Find eligible trainees efficiently using parallel processing
                var eligibleTrainees = traineeAssignments
                    .Where(ta => !traineeWithCerts.Contains(ta.TraineeId))
                    .AsParallel()
                    .Where(ta =>
                    {
                        if (!gradesByTraineeAssign.TryGetValue(ta.TraineeAssignId, out var grades))
                            return false;

                        return grades.Count() == subjectCount && grades.All(g => g.gradeStatus == GradeStatus.Pass);
                    })
                    .ToList();

                _logger.LogInformation($"Found {eligibleTrainees.Count()} eligible trainees for new certificates");

                if (!eligibleTrainees.Any())
                {
                    return createdCertificates;
                }

                // 8. Generate certificates in batch with efficient processing
                var certToCreate = new List<Certificate>();
                var certToUpdate = new List<Certificate>();
                var generationTasks = eligibleTrainees.Select(async ta =>
                {
                    if (!traineeDict.TryGetValue(ta.TraineeId, out var trainee))
                        return null;

                    try
                    {
                        var grades = gradesByTraineeAssign[ta.TraineeAssignId];

                        if (course.CourseLevel == CourseLevel.Recurrent)
                        {
                            var initialCourseId = course.RelatedCourseId;

                            if (!string.IsNullOrEmpty(initialCourseId))
                            {
                                var initialCert = (await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                                    c.UserId == trainee.UserId &&
                                    c.CourseId == initialCourseId &&
                                    c.Status == CertificateStatus.Active))
                                    .OrderByDescending(c => c.IssueDate)
                                    .FirstOrDefault();

                                if (initialCert != null)
                                {
                                    initialCert.ExpirationDate = (initialCert.ExpirationDate ?? DateTime.Now).AddYears(2);
                                    initialCert.IssueByUserId = issuedByUserId;
                                    initialCert.IssueDate = DateTime.Now;
                                    initialCert.Status = CertificateStatus.Pending;

                                    lock (certToUpdate)  // for thread safety in parallel
                                    {
                                        certToUpdate.Add(initialCert);
                                    }

                                    await _unitOfWork.CertificateRepository.UpdateAsync(initialCert);

                                    return initialCert; // not new, but added to report
                                }
                                else
                                {
                                    _logger.LogWarning($"No Initial certificate found for recurrent trainee {trainee.UserId} in course {course.CourseName}");
                                    return null;
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Recurrent course {course.CourseId} has no related initial course specified");
                                return null;
                            }
                        }
                        else
                        {
                            // Generate new certificate (Initial case)
                            return await GenerateCertificateAsync(
                                trainee, course, templateId, templateHtml, templateType, grades, issuedByUserId, issueDate);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to process certificate for trainee {ta.TraineeId}");
                        return null;
                    }
                });

                var certificates = (await Task.WhenAll(generationTasks))
                    .Where(c => c != null)
                    .ToList();

                // Thêm các certificate mới vào danh sách cần tạo
                foreach (var cert in certificates)
                {
                    if (!certToUpdate.Contains(cert))
                    {
                        certToCreate.Add(cert);
                    }
                }

                // 9. Save to database with transaction - xử lý cả certToCreate và certToUpdate
                await _unitOfWork.ExecuteWithStrategyAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        if (certToCreate.Any())
                        {
                            _logger.LogInformation($"Saving {certToCreate.Count} new certificates to database");
                            await _unitOfWork.CertificateRepository.AddRangeAsync(certToCreate);
                        }

                        // Lưu tất cả thay đổi (bao gồm cả certificate đã cập nhật thông qua UpdateAsync)
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        // Gộp cả certificate mới và certificate cập nhật để trả về kết quả
                        var allProcessedCertificates = new List<Certificate>();
                        allProcessedCertificates.AddRange(certToCreate);
                        allProcessedCertificates.AddRange(certToUpdate);

                        createdCertificates = _mapper.Map<List<CertificateModel>>(allProcessedCertificates);

                        // 10. Notify HeadMasters efficiently
                        await NotifyTrainingStaffsAsync(createdCertificates.Count, course.CourseName);

                        _logger.LogInformation($"Successfully processed {createdCertificates.Count} certificates for course {courseId} ({certToCreate.Count} new, {certToUpdate.Count} renewed)");
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "Transaction failed during certificate creation/renewal");
                        throw;
                    }
                });

                return createdCertificates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AutoGenerateCertificatesForPassedTraineesAsync for course {courseId}");
                throw new Exception("Failed to auto-generate certificates", ex);
            }
        }
        #endregion

        #region Create Certificate Manually
        /// <summary>
        /// Manually generates a certificate for a specific trainee in a course
        /// </summary>
        /// <param name="userId">The ID of the trainee receiving the certificate</param>
        /// <param name="courseId">The ID of the course</param>
        /// <param name="issuedByUserId">The ID of the user issuing the certificate</param>
        /// <returns>The created certificate model or null if failed</returns>
        public async Task<CertificateModel> CreateCertificateManuallyAsync(string userId, string courseId, string issuedByUserId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(issuedByUserId))
                throw new ArgumentException("UserId, CourseId and IssuedByUserId are required");

            try
            {
                // Check if trainee already has a certificate for this course
                var existingCertificate = await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                    c.UserId == userId && c.CourseId == courseId && c.Status == CertificateStatus.Active);

                if (existingCertificate.Any())
                {
                    throw new InvalidOperationException($"Trainee already has an active certificate for this course");
                }


                // Get course data
                var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                if (course == null || !course.CourseSubjectSpecialties.Any() || course.Status != CourseStatus.Approved)
                {
                    throw new Exception($"Course with ID {courseId} not found or not active or has no subjects!");
                }

                // Verify trainee is enrolled in this course
                var traineeAssignment = await _traineeAssignRepository.GetTraineeAssignmentAsync(courseId, userId);
                if (traineeAssignment == null)
                {
                    throw new InvalidOperationException($"Trainee is not enrolled in this course");
                }

                // Get all grades for this trainee in this course
                var grades = await _gradeRepository.GetGradesByTraineeAssignIdAsync(traineeAssignment.TraineeAssignId);
                if (!grades.Any() || grades.Count() < course.CourseSubjectSpecialties.Count)
                {
                    throw new InvalidOperationException($"Trainee has not completed all subjects in this course");
                }

                if (grades.Any(g => g.gradeStatus != GradeStatus.Pass))
                {
                    throw new InvalidOperationException($"Trainee has not passed all subjects in this course");
                }

                // Get trainee data
                var trainee = await _userRepository.GetByIdAsync(userId);
                if (trainee == null)
                {
                    throw new Exception($"Trainee with ID {userId} not found");
                }

                // Get template data
                var templateId = await GetTemplateIdByCourseLevelAsync(course.CourseLevel);
                if (string.IsNullOrEmpty(templateId))
                {
                    throw new Exception($"No active template for course level {course.CourseLevel}");
                }

                var certificateTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(templateId);
                if (certificateTemplate == null || certificateTemplate.templateStatus != TemplateStatus.Active)
                {
                    throw new Exception($"Certificate template with ID {templateId} not found or not active");
                }

                var templateHtml = await GetCachedTemplateHtmlAsync(certificateTemplate.TemplateFile);
                var templateType = GetTemplateTypeFromName(certificateTemplate.TemplateName);

                // Handle recurrent course differently
                var issueDate = DateTime.Now;
                Certificate certificate;

                if (course.CourseLevel == CourseLevel.Recurrent)
                {
                    var initialCourseId = course.RelatedCourseId;

                    if (!string.IsNullOrEmpty(initialCourseId))
                    {
                        var initialCert = (await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                            c.UserId == trainee.UserId &&
                            c.CourseId == initialCourseId &&
                            c.Status == CertificateStatus.Active))
                            .OrderByDescending(c => c.IssueDate)
                            .FirstOrDefault();

                        if (initialCert != null)
                        {
                            initialCert.ExpirationDate = (initialCert.ExpirationDate ?? DateTime.Now).AddYears(2);
                            initialCert.IssueByUserId = issuedByUserId;
                            initialCert.IssueDate = issueDate;
                            initialCert.Status = CertificateStatus.Pending;
                            initialCert.CourseId = course.CourseId;
                            await _unitOfWork.CertificateRepository.UpdateAsync(initialCert);
                            await _unitOfWork.SaveChangesAsync();

                            certificate = initialCert;
                        }
                        else
                        {
                            _logger.LogWarning($"No Initial certificate found for recurrent trainee {trainee.UserId} in course {course.CourseName}");
                            throw new Exception($"No initial certificate found for recurrent course");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Recurrent course {course.CourseId} has no related initial course specified");
                        throw new Exception($"Recurrent course has no related initial course specified");
                    }
                }
                else
                {
                    // Generate new certificate (Initial case)
                    certificate = await GenerateCertificateAsync(
                        trainee, course, templateId, templateHtml, templateType, grades.ToList(), issuedByUserId, issueDate);

                    // Save to database
                    await _unitOfWork.CertificateRepository.AddAsync(certificate);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Send notification to HeadMaster
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Certificate Request",
                        $"A new certificate request for {trainee.FullName} in course '{course.CourseName}' needs your signature.",
                        "CertificateSignature"
                    );
                }

                // Return the certificate with SAS URL
                var certificateModel = _mapper.Map<CertificateModel>(certificate);
                certificateModel.CertificateURLwithSas = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromHours(1));

                return certificateModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateCertificateManuallyAsync for user {userId} in course {courseId}");
                throw new Exception("Failed to create certificate manually", ex);
            }
        }
        #endregion

        #region Get all certificate with Pending status
        public async Task<List<CertificateModel>> GetPendingCertificatesWithSasUrlAsync()
        {
            var pendingCertificates = await GetAllPendingCertificatesAsync();

            var updateTasks = pendingCertificates
                .Where(c => !string.IsNullOrEmpty(c.CertificateURL))
                .Select(async certificate =>
                {
                    certificate.CertificateURLwithSas = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromHours(1));
                });

            await Task.WhenAll(updateTasks);

            return pendingCertificates;
        }
        #endregion

        #region Get certificate by ID
        public async Task<CertificateModel> GetCertificateByIdAsync(string certificateId)
        {
            var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
            if (certificate == null)
                return null;
            var certificateModel = _mapper.Map<CertificateModel>(certificate);
            certificateModel.CertificateURLwithSas = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromMinutes(5), "r");
            return certificateModel;
        }
        #endregion

        #region Get all certificates by user ID with SAS URL
        public async Task<List<CertificateModel>> GetCertificatesByUserIdWithSasUrlAsync(string userId)
        {
            var certificates = await GetCertificatesByUserIdAsync(userId);
            var updateTasks = certificates
                .Where(c => !string.IsNullOrEmpty(c.CertificateURL))
                .Select(async certificate =>
                {
                    certificate.CertificateURLwithSas = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromHours(1));
                });
            await Task.WhenAll(updateTasks);
            return certificates;
        }
        #endregion

        #region Get all active certificates
        public async Task<List<CertificateModel>> GetActiveCertificatesWithSasUrlAsync()
        {
            var activeCertificates = await GetActiveCertificatesAsync();
            var updateTasks = activeCertificates
                .Where(c => !string.IsNullOrEmpty(c.CertificateURL))
                .Select(async certificate =>
                {
                    certificate.CertificateURLwithSas = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromHours(1));
                });
            await Task.WhenAll(updateTasks);
            return activeCertificates;
        }
        #endregion

        #region revoke certificate
        public async Task<(bool success, string message)> RevokeCertificateAsync(string certificateId, RevokeCertificateDTO dto)
        {
            var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
            if (certificate == null)
            {
                return (false, "Certificate not found");
            }

            if (certificate.IsRevoked || certificate.Status == CertificateStatus.Revoked)
            {
                return (false, "Certificate is already revoked");
            }
            if (certificate.Status != CertificateStatus.Active)
            {
                return (false, "Certificate must be active to revoke.");
            }
            certificate.IsRevoked = true;
            certificate.RevocationReason = dto.RevokeReason;
            certificate.RevocationDate = DateTime.Now;
            certificate.Status = CertificateStatus.Revoked;

            await _unitOfWork.CertificateRepository.UpdateAsync(certificate);
            await _notificationService.SendNotificationAsync(
                                certificate.UserId,
                                "Revoke trainee certificate",
                                $"Your certificate for this course {certificate.CourseId} has been revoked due to this reason {dto.RevokeReason}",
                                $"Revoke certificate."
                            );
            await _unitOfWork.SaveChangesAsync();
            
            return (true, "Certificate revoked successfully");
        }
        #endregion

        #region Get all revoked certificates with SAS URL
        public async Task<List<CertificateModel>> GetRevokedCertificatesWithSasUrlAsync()
        {
            // Step 1: Get all revoked certificates
            var revokedCertificates = await GetAllRevokeCertificatesAsync();

            // Step 2: Generate SAS URLs for each certificate
            var updateTasks = revokedCertificates
                .Where(c => !string.IsNullOrEmpty(c.CertificateURL))
                .Select(async certificate =>
                {
                    certificate.CertificateURLwithSas = await _blobService.GetBlobUrlWithSasTokenAsync(
                        certificate.CertificateURL,
                        TimeSpan.FromHours(1)
                    );
                });

            await Task.WhenAll(updateTasks);

            return revokedCertificates;
        }
        #endregion

        #region Get Certificate Renewal History
        /// <summary>
        /// Lấy lịch sử gia hạn của một chứng chỉ dựa trên ID chứng chỉ
        /// </summary>
        /// <param name="certificateId">ID của chứng chỉ cần xem lịch sử</param>
        /// <returns>Thông tin lịch sử gia hạn của chứng chỉ</returns>
        public async Task<CertificateRenewalHistoryModel> GetCertificateRenewalHistoryAsync(string certificateId)
        {
            if (string.IsNullOrEmpty(certificateId))
                throw new ArgumentException("Certificate ID is required");

            try
            {
                // 1. Lấy chứng chỉ hiện tại
                var currentCertificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
                if (currentCertificate == null)
                {
                    throw new KeyNotFoundException($"Certificate with ID {certificateId} not found");
                }

                // Kiểm tra trạng thái chứng chỉ
                if (currentCertificate.Status != CertificateStatus.Active &&
                    currentCertificate.Status != CertificateStatus.Pending)
                {
                    _logger.LogWarning($"Certificate with ID {certificateId} is not active or pending (Status: {currentCertificate.Status})");
                }

                // 2. Lấy thông tin khóa học
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(currentCertificate.CourseId);
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course with ID {currentCertificate.CourseId} not found");
                }

                // 3. Xác định khóa học gốc và các khóa học tái cấp chứng chỉ liên quan
                var isRecurrentCourse = course.CourseLevel == CourseLevel.Recurrent;

                // Khóa học ban đầu là khóa học hiện tại nếu là khóa Initial,
                // hoặc là khóa học được liên kết nếu là khóa Recurrent
                var originalCourseId = isRecurrentCourse ? course.RelatedCourseId : course.CourseId;

                if (isRecurrentCourse && string.IsNullOrEmpty(originalCourseId))
                {
                    _logger.LogWarning($"Recurrent course {course.CourseId} has no related initial course specified");
                }

                // Lấy danh sách tất cả các chứng chỉ liên quan 
                // (chứng chỉ ban đầu và các chứng chỉ tái cấp)
                var relatedCertificates = new List<Certificate>();

                // Thêm chứng chỉ từ khóa học hiện tại - loại bỏ chứng chỉ đã thu hồi
                var currentCourseCerts = await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                    c.UserId == currentCertificate.UserId &&
                    c.CourseId == currentCertificate.CourseId &&
                    c.Status != CertificateStatus.Revoked);
                relatedCertificates.AddRange(currentCourseCerts);

                // Nếu là khóa tái cấp, thêm chứng chỉ từ khóa ban đầu
                if (isRecurrentCourse && !string.IsNullOrEmpty(originalCourseId))
                {
                    var initialCourseCerts = await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                        c.UserId == currentCertificate.UserId &&
                        c.CourseId == originalCourseId &&
                        c.Status != CertificateStatus.Revoked);
                    relatedCertificates.AddRange(initialCourseCerts);
                }
                // Nếu là khóa ban đầu, tìm các chứng chỉ từ khóa tái cấp
                else if (!isRecurrentCourse)
                {
                    // Lấy các khóa học tái cấp liên quan đến khóa học hiện tại
                    var recurrentCourses = await _unitOfWork.CourseRepository.GetAllAsync(c =>
                        c.RelatedCourseId == course.CourseId && c.CourseLevel == CourseLevel.Recurrent);

                    foreach (var recurrentCourse in recurrentCourses)
                    {
                        var recurrentCerts = await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                            c.UserId == currentCertificate.UserId &&
                            c.CourseId == recurrentCourse.CourseId &&
                            c.Status != CertificateStatus.Revoked);
                        relatedCertificates.AddRange(recurrentCerts);
                    }
                }

                // Loại bỏ các phiên bản trùng lặp
                relatedCertificates = relatedCertificates.Distinct().ToList();

                // 4. Sắp xếp theo thời gian để tìm chứng chỉ gốc và các lần gia hạn
                var orderedCertificates = relatedCertificates
                    .OrderBy(c => c.IssueDate)
                    .ToList();

                if (!orderedCertificates.Any())
                {
                    _logger.LogWarning($"No certificates found for user {currentCertificate.UserId} related to course {course.CourseId}");

                    // Fallback: Sử dụng chứng chỉ hiện tại làm đối tượng duy nhất
                    if (currentCertificate.Status != CertificateStatus.Revoked)
                    {
                        orderedCertificates.Add(currentCertificate);
                    }
                    else
                    {
                        _logger.LogWarning($"Current certificate with ID {certificateId} is revoked");
                        // Nếu chứng chỉ hiện tại đã bị thu hồi, trả về lịch sử trống
                        return new CertificateRenewalHistoryModel
                        {
                            CertificateId = currentCertificate.CertificateId,
                            CertificateCode = currentCertificate.CertificateCode,
                            CourseId = course.CourseId,
                            CourseName = course.CourseName,
                            RenewalHistory = new List<RenewalEventModel>()
                        };
                    }
                }

                // Chứng chỉ gốc là chứng chỉ đầu tiên theo thời gian
                var originalCertificate = orderedCertificates.First();

                // 5. Lấy thông tin người cấp chứng chỉ
                var allUserIds = orderedCertificates
                    .Select(c => c.IssueByUserId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                var users = await _userRepository.GetUsersByIdsAsync(allUserIds);
                var userDict = users.ToDictionary(u => u.UserId);

                // 6. Tạo đối tượng lịch sử gia hạn
                var renewalHistory = new CertificateRenewalHistoryModel
                {
                    CertificateId = currentCertificate.CertificateId,
                    CertificateCode = currentCertificate.CertificateCode,
                    OriginalIssueDate = originalCertificate.IssueDate,
                    CurrentIssueDate = currentCertificate.IssueDate,
                    CurrentExpirationDate = currentCertificate.ExpirationDate,
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    IssuedByUserId = originalCertificate.IssueByUserId,
                    IssuedByUserName = userDict.TryGetValue(originalCertificate.IssueByUserId, out var originalUser)
                        ? originalUser.FullName
                        : "Unknown User"
                };

                // 7. Xây dựng lịch sử gia hạn
                // Chỉ xem xét certificate sau certificate gốc (từ index 1 trở đi)
                // và đảm bảo rằng đây thực sự là renewal (dựa vào thời gian hết hạn)
                for (int i = 1; i < orderedCertificates.Count; i++)
                {
                    var currentCert = orderedCertificates[i];
                    var previousCert = orderedCertificates[i - 1];

                    // Xác định một chứng chỉ là gia hạn khi:
                    // 1. Nếu là chứng chỉ mới cho khóa học tái cấp liên quan đến khóa học gốc
                    // 2. Thời gian cấp mới phải muộn hơn thời gian cấp trước đó đáng kể
                    //    (ít nhất 3 tháng, tránh trường hợp sớm hơn khi chỉ là cấp lại)
                    bool isRenewal = false;

                    // Trường hợp khóa học tái cấp liên quan đến khóa học ban đầu
                    if (currentCert.CourseId != previousCert.CourseId)
                    {
                        // Kiểm tra xem currentCert có phải là chứng chỉ từ khóa tái cấp không
                        var currentCertCourse = await _unitOfWork.CourseRepository.GetByIdAsync(currentCert.CourseId);
                        if (currentCertCourse != null && currentCertCourse.CourseLevel == CourseLevel.Recurrent)
                        {
                            isRenewal = true;
                        }
                    }
                    // Trường hợp cấp lại chứng chỉ trong cùng một khóa học (thường là gia hạn)
                    else
                    {
                        // Kiểm tra nếu thời gian cấp mới gần với thời gian hết hạn của chứng chỉ cũ
                        // hoặc sau thời gian hết hạn cũ, thì đây là gia hạn
                        var previousExpiryDate = previousCert.ExpirationDate ?? previousCert.IssueDate.AddYears(2);
                        var timeDiff = currentCert.IssueDate - previousCert.IssueDate;

                        // Nếu đã qua ít nhất 1 năm kể từ lúc cấp ban đầu, hoặc gần đến thời gian hết hạn (trong vòng 6 tháng)
                        isRenewal = timeDiff.TotalDays > 365 ||
                                   (previousExpiryDate - currentCert.IssueDate).TotalDays <= 180;
                    }

                    // Nếu là gia hạn, thêm vào lịch sử
                    if (isRenewal)
                    {
                        var previousExpiryDate = previousCert.ExpirationDate ?? previousCert.IssueDate.AddYears(2);
                        var newExpiryDate = currentCert.ExpirationDate ?? currentCert.IssueDate.AddYears(2);

                        var renewalEvent = new RenewalEventModel
                        {
                            RenewalDate = currentCert.IssueDate,
                            PreviousExpirationDate = previousExpiryDate,
                            NewExpirationDate = newExpiryDate,
                            RenewedByUserId = currentCert.IssueByUserId,
                            RenewedByUserName = userDict.TryGetValue(currentCert.IssueByUserId, out var user)
                                ? user.FullName
                                : "Unknown User"
                        };

                        renewalHistory.RenewalHistory.Add(renewalEvent);
                    }
                }

                // Sắp xếp lịch sử gia hạn theo thời gian, mới nhất lên đầu
                renewalHistory.RenewalHistory = renewalHistory.RenewalHistory
                    .OrderByDescending(r => r.RenewalDate)
                    .ToList();

                return renewalHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting renewal history for certificate {certificateId}");
                throw new Exception("Failed to retrieve certificate renewal history", ex);
            }
        }
        #endregion

        #region Get User Certificate Renewal History
        /// <summary>
        /// Lấy lịch sử gia hạn của tất cả chứng chỉ của một người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Danh sách lịch sử gia hạn chứng chỉ của người dùng</returns>
        public async Task<List<CertificateRenewalHistoryModel>> GetUserCertificateRenewalHistoryAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required");

            try
            {
                // Lấy tất cả các chứng chỉ của người dùng - loại bỏ chứng chỉ đã thu hồi
                var certificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                    c => c.UserId == userId && c.Status != CertificateStatus.Revoked);

                if (!certificates.Any())
                {
                    return new List<CertificateRenewalHistoryModel>();
                }

                var result = new List<CertificateRenewalHistoryModel>();
                var uniqueCertificateCodes = certificates
                    .Select(c => c.CertificateCode)
                    .Distinct()
                    .ToList();

                // Chỉ lấy các chứng chỉ mới nhất (theo từng mã chứng chỉ)
                foreach (var code in uniqueCertificateCodes)
                {
                    var latestCertificate = certificates
                        .Where(c => c.CertificateCode == code)
                        .OrderByDescending(c => c.IssueDate)
                        .FirstOrDefault();

                    if (latestCertificate != null)
                    {
                        var renewalHistory = await GetCertificateRenewalHistoryAsync(latestCertificate.CertificateId);
                        result.Add(renewalHistory);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting renewal history for user {userId}");
                throw new Exception("Failed to retrieve user certificate renewal history", ex);
            }
        }
        #endregion

        #region Helper Methods
        private async Task<string> GetCachedTemplateHtmlAsync(string templateFileUrl)
        {
            string cacheKey = string.Format(TEMPLATE_HTML_CACHE_KEY, templateFileUrl.GetHashCode());

            if (!_memoryCache.TryGetValue(cacheKey, out string templateHtml))
            {
                _logger.LogInformation($"Template cache miss for {templateFileUrl}, loading from blob storage");
                templateHtml = await GetTemplateHtmlFromBlobAsync(templateFileUrl);

                // Cache the template HTML for future use
                _memoryCache.Set(cacheKey, templateHtml, TimeSpan.FromMinutes(TEMPLATE_CACHE_MINUTES));
            }

            return templateHtml;
        }

        private async Task<string> GetTemplateHtmlFromBlobAsync(string templateFileUrl)
        {
            try
            {
                // Parse URL to get account endpoint
                Uri blobUri = new Uri(templateFileUrl);
                string accountUrl = $"{blobUri.Scheme}://{blobUri.Host}";
                string containerName = blobUri.Segments[1].TrimEnd('/');
                string blobName = blobUri.AbsolutePath.Substring(blobUri.AbsolutePath.IndexOf(containerName) + containerName.Length + 1);

                // Use DefaultAzureCredential (will use Managed Identity when running on Azure)
                BlobServiceClient blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new DefaultAzureCredential());
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                using var memoryStream = new MemoryStream();
                var downloadOperation = await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;

                using var reader = new StreamReader(memoryStream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading template from blob: {templateFileUrl}");
                throw new Exception($"Failed to load certificate template", ex);
            }
        }

        private async Task<string> GetTemplateIdByCourseLevelAsync(CourseLevel courseLevel)
        {
            string cacheKey = $"template_id_{courseLevel}";

            if (!_memoryCache.TryGetValue(cacheKey, out string templateId))
            {
                var templates = await _unitOfWork.CertificateTemplateRepository.GetAllAsync(
                    t => t.templateStatus == TemplateStatus.Active);
                if (!templates.Any())
                {
                    _logger.LogWarning($"No active certificate templates found");
                    return null;
                }

                // Filter templates based on course level
                var matchingTemplates = new List<CertificateTemplate>();

                switch (courseLevel)
                {
                    case CourseLevel.Initial:
                        matchingTemplates = templates.Where(t => t.TemplateName.Contains("Initial")).ToList();
                        break;
                    case CourseLevel.Relearn:
                        matchingTemplates = templates.Where(t => t.TemplateName.Contains("Initial")).ToList();
                        break;
                    default:
                        matchingTemplates = templates.ToList();
                        break;
                }

                if (!matchingTemplates.Any())
                    return null;

                // If multiple matching templates exist, select the one with the highest sequence number
                templateId = matchingTemplates
                    .OrderByDescending(t =>
                    {
                        // Extract the sequence number (last 3 digits after last dash)
                        var parts = t.CertificateTemplateId.Split('-');
                        if (parts.Length >= 3 && int.TryParse(parts[2], out int sequenceNumber))
                            return sequenceNumber;
                        return 0;
                    })
                    .FirstOrDefault()?.CertificateTemplateId;

                // Cache the template ID for 30 minutes
                if (templateId != null)
                    _memoryCache.Set(cacheKey, templateId, TimeSpan.FromMinutes(30));
            }

            return templateId;
        }

        private string GetTemplateTypeFromName(string templateName)
        {
            if (templateName.Contains("Initial")) return "Initial";
            if (templateName.Contains("Professional")) return "Professional";
            return "Standard";
        }

        private async Task<Certificate> GenerateCertificateAsync(
            User trainee,
            Course course,
            string templateId,
            string templateHtml,
            string templateType,
            List<Grade> grades,
            string issuedByUserId,
            DateTime issueDate)
        {
            string certificateCode = GenerateCertificateCode(course, trainee);
            string modifiedHtml = await PopulateTemplateAsync(templateHtml, trainee, course, issueDate, certificateCode, grades, templateType);
            string certificateFileName = $"certificate_{certificateCode}_{DateTime.Now:yyyyMMddHHmmss}.html";
            string certificateUrl = await SaveCertificateToBlob(modifiedHtml, certificateFileName);
            var userExist = await _unitOfWork.UserRepository.GetByIdAsync(trainee.UserId);
            var courseExist = await _unitOfWork.CourseRepository.GetByIdAsync(course.CourseId);
            var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
            var certificateId = Guid.NewGuid().ToString();
            foreach (var director in directors)
            {
                await _notificationService.SendNotificationAsync(
                    director.UserId,
                    "New Request Submitted",
                    $"A new sign Request for certificateId {certificateId} need to be signed.",
                    "Request"
                );
            }
            return new Certificate
            {
                CertificateId = certificateId,
                CertificateCode = certificateCode,
                UserId = trainee.UserId,
                CourseId = course.CourseId,
                CertificateTemplateId = templateId,
                IssueByUserId = issuedByUserId,
                IssueDate = issueDate,
                ExpirationDate = issueDate.AddYears(3),
                Status = CertificateStatus.Pending,
                CertificateURL = certificateUrl,
                IsRevoked = false,
                Course = courseExist,
                User = userExist,
                SignDate = DateTime.Now
            };
        }

        private string GenerateCertificateCode(Course course, User trainee)
        {
            // Create a unique code format like: OCMS-{CourseLevel}-{Year}-{Month}-{Hash}
            string courseLevel = course.CourseLevel.ToString().Substring(0, 3).ToUpper();
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString("D2");

            // Generate a hash from trainee ID + course ID
            string hash = (trainee.UserId + course.CourseId).GetHashCode().ToString("X").Substring(0, 5);

            return $"OCMS-{courseLevel}-{year}-{month}-{hash}";
        }

        private async Task<string> SaveCertificateToBlob(string htmlContent, string fileName)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
            var blobUrl = await _blobService.UploadFileAsync("certificates", fileName, stream, "text/html");
            // Trả về URL gốc không có SAS token
            return _blobService.GetBlobUrlWithoutSasToken(blobUrl);
        }

        private async Task<string> PopulateTemplateAsync(
            string templateHtml,
            User trainee,
            Course course,
            DateTime issueDate,
            string certificateCode,
            IEnumerable<Grade> grades,
            string templateType)
        {
            // Get general info
            var startDate = issueDate.ToString("dd/MM/yyyy");
            var endDate = issueDate.AddYears(3).ToString("dd/MM/yyyy");
            double averageScore = grades.Average(g => g.TotalScore);
            string gradeResult = DetermineGradeResult(averageScore);
            string avatarBase64 = await GetBase64AvatarAsync(trainee.AvatarUrl);

            // Replace common placeholders
            var result = templateHtml
                .Replace("[HỌ VÀ TÊN]", trainee.FullName)
                .Replace("[Họ tên]", trainee.FullName)
                .Replace("[NGÀY SINH]", trainee.DateOfBirth.ToString("dd/MM/yyyy"))
                .Replace("[Ngày sinh]", trainee.DateOfBirth.ToString("dd/MM/yyyy"))
                .Replace("[Nơi sinh]", trainee.Address ?? "N/A")
                .Replace("[TÊN KHÓA HỌC]", course.CourseName)
                .Replace("[Tên khóa học]", course.CourseName)
                .Replace("[NGÀY BẮT ĐẦU]", startDate)
                .Replace("[Ngày bắt đầu]", startDate)
                .Replace("[NGÀY KẾT THÚC]", endDate)
                .Replace("[Ngày kết thúc]", endDate)
                .Replace("[MÃ CHỨNG CHỈ]", certificateCode)
                .Replace("[Mã chứng chỉ]", certificateCode);

            // Replace template-specific placeholders
            if (templateType == "Initial")
            {
                result = result.Replace("[LOẠI TỐT NGHIỆP]", gradeResult);
            }

            // Update date in signature
            var currentDate = DateTime.Now;
            result = Regex.Replace(result,
                @"ngày\s+\d+\s+tháng\s+\d+\s+năm\s+\d+",
                $"ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}");

            // Replace the image tag with appropriate 3x4 aspect ratio dimensions
            result = Regex.Replace(result,
                "<img src=\"placeholder-photo.jpg\".*?>",
                $"<img src=\"{avatarBase64}\" alt=\"{trainee.FullName}\" style=\"width: 150px; height: 204.8px; object-fit: cover;\">");

            return result;
        }

        private string DetermineGradeResult(double averageScore)
        {
            if (averageScore >= 9) return "Xuất Sắc / Excellent";
            if (averageScore >= 8) return "Giỏi / Very Good";
            if (averageScore >= 7) return "Khá / Good";
            if (averageScore >= 6) return "Trung Bình / Average";
            return "Đạt / Pass";
        }

        private async Task<string> GetBase64AvatarAsync(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
                return await GetDefaultBase64AvatarAsync();

            try
            {
                // Check cache first
                string cacheKey = $"avatar_base64_{avatarUrl.GetHashCode()}";
                if (_memoryCache.TryGetValue(cacheKey, out string cachedAvatar))
                {
                    return cachedAvatar;
                }

                // Đảm bảo URL có SAS token cập nhật
                var urlWithSasToken = await _blobService.GetBlobUrlWithSasTokenAsync(avatarUrl, TimeSpan.FromMinutes(5));

                // Sử dụng HttpClient để tải avatar từ URL với SAS token
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(urlWithSasToken);
                    response.EnsureSuccessStatusCode();

                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    string base64String = Convert.ToBase64String(bytes);
                    string result = $"data:{contentType};base64,{base64String}";

                    // Cache và trả về kết quả
                    _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(60));
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating base64 for avatar");
                return await GetDefaultBase64AvatarAsync();
            }
        }

        private async Task<string> GetDefaultBase64AvatarAsync()
        {
            string cacheKey = "default_avatar_base64";

            if (_memoryCache.TryGetValue(cacheKey, out string cachedAvatar))
            {
                return cachedAvatar;
            }

            // Create a default avatar in Base64 format
            string defaultAvatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "default-avatar.jpg");

            if (File.Exists(defaultAvatarPath))
            {
                byte[] bytes = await File.ReadAllBytesAsync(defaultAvatarPath);
                string base64String = Convert.ToBase64String(bytes);
                string result = $"data:image/jpeg;base64,{base64String}";

                // Cache the result indefinitely (it won't change)
                _memoryCache.Set(cacheKey, result, TimeSpan.FromDays(365));

                return result;
            }

            // If no default file exists, return a simple SVG image in Base64
            string svgBase64 = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDAiIGhlaWdodD0iMTAwIiB2aWV3Qm94PSIwIDAgMTAwIDEwMCI+PHJlY3Qgd2lkdGg9IjEwMCIgaGVpZ2h0PSIxMDAiIGZpbGw9IiNlMGUwZTAiLz48Y2lyY2xlIGN4PSI1MCIgY3k9IjM1IiByPSIyMCIgZmlsbD0iIzlFOUU5RSIvPjxwYXRoIGQ9Ik0yNSw4NSBDMjUsNjAgNzUsNjAgNzUsODUgWiIgZmlsbD0iIzlFOUU5RSIvPjwvc3ZnPg==";
            _memoryCache.Set(cacheKey, svgBase64, TimeSpan.FromDays(365));

            return svgBase64;
        }

        private async Task NotifyTrainingStaffsAsync(int certificateCount, string courseName)
        {
            if (certificateCount <= 0)
                return;

            try
            {
                var trainingStaff = await _userRepository.GetUsersByRoleAsync("Training staff");

                if (!trainingStaff.Any())
                {
                    _logger.LogWarning("No HeadMasters found to notify about certificates");
                    return;
                }

                // Create a more detailed notification message
                string title = "Certificates Pending Digital Signature";
                string message = $"{certificateCount} new certificate(s) for course '{courseName}' have been generated and require to sign with digital signature. " +
                                 $"Please review and request HeadMaster to sign these certificates at your earliest convenience.";

                var notificationTasks = trainingStaff.Select(hm => _notificationService.SendNotificationAsync(
                    hm.UserId,
                    title,
                    message,
                    "CertificateSignature"
                ));

                await Task.WhenAll(notificationTasks);
                _logger.LogInformation($"Notification sent to {trainingStaff.Count()} HeadMasters for {certificateCount} certificates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications to HeadMasters");
                // We don't throw here as notification failure shouldn't break certificate generation
            }
        }

        private async Task<List<CertificateModel>> GetCertificatesByUserIdAsync(string userId)
        {
            var certificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.UserId == userId);
            if (certificates == null)
            {
                throw new KeyNotFoundException("This user doesn't have any certificates.");
            }
            return _mapper.Map<List<CertificateModel>>(certificates);
        }

        private async Task<List<CertificateModel>> GetAllPendingCertificatesAsync()
        {
            var pendingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.Status == CertificateStatus.Pending);

            return _mapper.Map<List<CertificateModel>>(pendingCertificates);
        }
        private async Task<List<CertificateModel>> GetAllRevokeCertificatesAsync()
        {
            var pendingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.Status == CertificateStatus.Revoked);

            return _mapper.Map<List<CertificateModel>>(pendingCertificates);
        }

        private async Task<List<CertificateModel>> GetActiveCertificatesAsync()
        {
            var pendingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.Status == CertificateStatus.Active);
            return _mapper.Map<List<CertificateModel>>(pendingCertificates);
        }
        #endregion
    }
}