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

        #region Auto Generate Certificates
        public async Task<List<CertificateModel>> AutoGenerateCertificatesForPassedTraineesAsync(string courseId, string issuedByUserId)
        {
            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(issuedByUserId))
                throw new ArgumentException("CourseId and IssuedByUserId are required");

            var createdCertificates = new List<CertificateModel>();

            try
            {
                var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                if (course == null || !course.SubjectSpecialties.Any() || course.Status != CourseStatus.Approved)
                {
                    throw new Exception($"Course with ID {courseId} not found or not active or has no subjects!");
                }

                int subjectCount = course.SubjectSpecialties.Count;

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

                var traineeAssignments = await _traineeAssignRepository.GetTraineeAssignmentsByCourseIdAsync(course.CourseId);
                var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.CourseId == courseId);
                var allGrades = await _gradeRepository.GetGradesByCourseIdAsync(courseId);

                var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
                var traineeIds = traineeAssignments.Select(ta => ta.TraineeId).Distinct().ToList();

                _logger.LogInformation($"Found {traineeIds.Count()} trainees enrolled in course, {traineeWithCerts.Count} already have certificates");

                var trainees = await _userRepository.GetUsersByIdsAsync(traineeIds);
                var traineeDict = trainees.ToDictionary(t => t.UserId);

                var gradesByTraineeAssign = allGrades
                    .GroupBy(g => g.TraineeAssignID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var issueDate = DateTime.Now;

                // Group trainee assignments by trainee and specialty
                var traineeAssignmentsByTrainee = traineeAssignments
                    .GroupBy(ta => ta.TraineeId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var courseSubjectSpecialtiesBySpecialty = course.SubjectSpecialties
                    .GroupBy(css => css.SpecialtyId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var eligibleTrainees = new List<(string TraineeId, string SpecialtyId)>();

                foreach (var traineeGroup in traineeAssignmentsByTrainee)
                {
                    var traineeId = traineeGroup.Key;
                    var assignments = traineeGroup.Value;

                    // Get specialty from the first assignment's ClassSubject
                    var specialtyId = assignments.First().ClassSubject.Subject.SubjectId;
                    if (assignments.Any(ta => ta.ClassSubject.Subject.SubjectId != specialtyId))
                    {
                        _logger.LogWarning($"Trainee {traineeId} has assignments for multiple specialties in course {courseId}");
                        continue;
                    }

                    if (!courseSubjectSpecialtiesBySpecialty.TryGetValue(specialtyId, out var requiredCss))
                    {
                        _logger.LogWarning($"Specialty {specialtyId} not found in course {courseId}");
                        continue;
                    }

                    var traineeGrades = assignments.SelectMany(ta => gradesByTraineeAssign.GetValueOrDefault(ta.TraineeAssignId, new List<Grade>()));
                    var passedCssIds = traineeGrades.Where(g => g.gradeStatus == GradeStatus.Pass)
                                                    .Select(g => g.TraineeAssign.ClassSubjectId)
                                                    .ToHashSet();
                    var requiredCssIds = requiredCss.Select(css => css.SubjectSpecialtyId).ToHashSet();

                    // Explicitly construct the tuple with named parameters
                    if (requiredCssIds.All(id => passedCssIds.Contains(id)) && !traineeWithCerts.Contains(traineeId))
                    {
                        eligibleTrainees.Add((TraineeId: traineeId, SpecialtyId: specialtyId));
                    }
                }

                _logger.LogInformation($"Found {eligibleTrainees.Count()} eligible trainees for new certificates");

                if (!eligibleTrainees.Any())
                {
                    return createdCertificates;
                }

                var certToCreate = new List<Certificate>();
                var certToUpdate = new List<Certificate>();
                var generationTasks = eligibleTrainees.Select(async tuple =>
                {
                    var (traineeId, specialtyId) = tuple;
                    if (!traineeDict.TryGetValue(traineeId, out var trainee))
                        return null;

                    try
                    {
                        var grades = traineeAssignmentsByTrainee[traineeId]
                            .SelectMany(ta => gradesByTraineeAssign.GetValueOrDefault(ta.TraineeAssignId, new List<Grade>()))
                            .ToList();

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
                                    initialCert.ExpirationDate = DateTime.Now.AddYears(2);
                                    initialCert.CourseId = course.CourseId; // Update to recurrent course ID
                                    initialCert.IssueByUserId = issuedByUserId;
                                    initialCert.IssueDate = DateTime.Now;
                                    initialCert.Status = CertificateStatus.Pending;
                                    initialCert.SpecialtyId = specialtyId; // Set SpecialtyId for recurrent course

                                    lock (certToUpdate)
                                    {
                                        certToUpdate.Add(initialCert);
                                    }

                                    await _unitOfWork.CertificateRepository.UpdateAsync(initialCert);

                                    return initialCert;
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
                            return await GenerateCertificateAsync(
                                trainee, course, templateId, templateHtml, templateType, grades, issuedByUserId, issueDate, specialtyId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to process certificate for trainee {traineeId}");
                        return null;
                    }
                });

                var certificates = (await Task.WhenAll(generationTasks))
                    .Where(c => c != null)
                    .ToList();

                foreach (var cert in certificates)
                {
                    if (!certToUpdate.Contains(cert))
                    {
                        certToCreate.Add(cert);
                    }
                }

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

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        var allProcessedCertificates = new List<Certificate>();
                        allProcessedCertificates.AddRange(certToCreate);
                        allProcessedCertificates.AddRange(certToUpdate);

                        createdCertificates = _mapper.Map<List<CertificateModel>>(allProcessedCertificates);

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
        public async Task<CertificateModel> CreateCertificateManuallyAsync(string userId, string courseId, string issuedByUserId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(issuedByUserId))
                throw new ArgumentException("UserId, CourseId and IssuedByUserId are required");

            try
            {
                var existingCertificate = await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                    c.UserId == userId && c.CourseId == courseId && c.Status == CertificateStatus.Active);

                if (existingCertificate.Any())
                {
                    throw new InvalidOperationException($"Trainee already has an active certificate for this course");
                }

                var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                if (course == null || !course.SubjectSpecialties.Any() || course.Status != CourseStatus.Approved)
                {
                    throw new Exception($"Course with ID {courseId} not found or not active or has no subjects!");
                }

                var trainee = await _userRepository.GetByIdAsync(userId);
                if (trainee == null)
                {
                    throw new Exception($"Trainee with ID {userId} not found");
                }

                // Get specialty from trainee info
                string specialtyId = trainee.SpecialtyId;
                if (string.IsNullOrEmpty(specialtyId))
                {
                    throw new InvalidOperationException($"Trainee does not have a specialty assigned");
                }

                // Get trainee assignments for this course with the assigned specialty
                var traineeAssignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync(
                    ta => ta.TraineeId == userId &&
                         ta.ClassSubject.Subject.SubjectId == specialtyId &&
                         ta.RequestStatus == RequestStatus.Approved);

                if (!traineeAssignments.Any())
                {
                    throw new InvalidOperationException($"Trainee is not enrolled in this course with their specialty");
                }

                // Get subject specialties for the trainee's specialty
                var courseSubjectSpecialties = course.SubjectSpecialties
                    .Where(css => css.SpecialtyId == specialtyId)
                    .ToList();

                if (!courseSubjectSpecialties.Any())
                {
                    throw new InvalidOperationException($"No subjects found for trainee's specialty {specialtyId} in this course");
                }

                // Check if trainee is enrolled in all required subjects for their specialty
                var assignedSubjectIds = traineeAssignments
                    .Select(ta => ta.ClassSubject.SubjectId)
                    .Distinct()
                    .ToList();

                var requiredSubjectIds = courseSubjectSpecialties
                    .Select(css => css.SubjectId)
                    .Distinct()
                    .ToList();

                var missingSubjects = requiredSubjectIds.Except(assignedSubjectIds).ToList();
                if (missingSubjects.Any())
                {
                    throw new InvalidOperationException($"Trainee is not enrolled in all required subjects for their specialty");
                }

                // Check grades for all subjects
                foreach (var traineeAssignment in traineeAssignments)
                {
                    var grades = await _gradeRepository.GetGradesByTraineeAssignIdAsync(traineeAssignment.TraineeAssignId);

                    if (!grades.Any())
                    {
                        throw new InvalidOperationException($"Trainee has not completed subject '{traineeAssignment.ClassSubject.Subject?.SubjectName ?? traineeAssignment.ClassSubjectId}' in this course");
                    }

                    if (grades.Any(g => g.gradeStatus != GradeStatus.Pass))
                    {
                        throw new InvalidOperationException($"Trainee has not passed subject '{traineeAssignment.ClassSubject.Subject?.SubjectName ?? traineeAssignment.ClassSubjectId}' in this course");
                    }
                }

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

                var issueDate = DateTime.Now;
                Certificate certificate;

                // Get all grades for certificate creation
                var allGrades = new List<Grade>();
                foreach (var traineeAssign in traineeAssignments)
                {
                    var assignGrades = await _gradeRepository.GetGradesByTraineeAssignIdAsync(traineeAssign.TraineeAssignId);
                    allGrades.AddRange(assignGrades);
                }

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
                            initialCert.ExpirationDate = DateTime.Now.AddYears(2);
                            initialCert.IssueByUserId = issuedByUserId;
                            initialCert.IssueDate = issueDate;
                            initialCert.Status = CertificateStatus.Pending;
                            initialCert.CourseId = course.CourseId;
                            initialCert.SpecialtyId = specialtyId;
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
                    certificate = await GenerateCertificateAsync(
                        trainee, course, templateId, templateHtml, templateType, allGrades, issuedByUserId, issueDate, specialtyId);

                    await _unitOfWork.CertificateRepository.AddAsync(certificate);
                    await _unitOfWork.SaveChangesAsync();
                }

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

        #region Create Certificate with Custom Grade
        public async Task<CertificateModel> CreateCertificateWithCustomGradesAsync(string courseId, string userId, string issuedByUserId, List<Grade> customGrades)
        {
            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(issuedByUserId))
                throw new ArgumentException("CourseId, UserId and IssuedByUserId are required");

            try
            {
                var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                if (course == null || !course.SubjectSpecialties.Any() || course.Status != CourseStatus.Approved)
                {
                    throw new Exception($"Course with ID {courseId} not found or not active or has no subjects!");
                }

                var trainee = await _userRepository.GetByIdAsync(userId);
                if (trainee == null)
                {
                    throw new Exception($"Trainee with ID {userId} not found");
                }

                string specialtyId = trainee.SpecialtyId;
                if (string.IsNullOrEmpty(specialtyId))
                {
                    throw new InvalidOperationException($"Trainee does not have a specialty assigned");
                }

                var certToCreate = new List<Certificate>();
                var certToUpdate = new List<Certificate>();
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
                            initialCert.ExpirationDate = DateTime.Now.AddYears(2);
                            initialCert.CourseId = course.CourseId; // Update to recurrent course ID
                            initialCert.IssueByUserId = issuedByUserId;
                            initialCert.IssueDate = DateTime.Now;
                            initialCert.Status = CertificateStatus.Pending;
                            initialCert.SpecialtyId = specialtyId; // Set SpecialtyId for recurrent course

                            lock (certToUpdate)
                            {
                                certToUpdate.Add(initialCert);
                            }

                            await _unitOfWork.CertificateRepository.UpdateAsync(initialCert);

                            return _mapper.Map<CertificateModel>(initialCert);
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

                var issueDate = DateTime.Now;

                // Use custom grades directly
                var certificate = await GenerateCertificateAsync(
                    trainee, course, templateId, templateHtml, templateType,
                    customGrades, issuedByUserId, issueDate, specialtyId);

                await _unitOfWork.CertificateRepository.AddAsync(certificate);
                await _unitOfWork.SaveChangesAsync();

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

                var certificateModel = _mapper.Map<CertificateModel>(certificate);
                certificateModel.CertificateURLwithSas = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromHours(1));

                return certificateModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateCertificateWithCustomGradesAsync for user {userId} in course {courseId}");
                throw new Exception("Failed to create certificate with custom grades", ex);
            }
        }
        #endregion

        #region Get All Pending Certificates
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

        #region Get Certificate By Id
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

        #region Get All Certificates By UserId
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

        #region Get All Active Certificates
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

        #region Revoke Certificate
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

        #region Get All Revoked Certificates
        public async Task<List<CertificateModel>> GetRevokedCertificatesWithSasUrlAsync()
        {
            var revokedCertificates = await GetAllRevokeCertificatesAsync();
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
        public async Task<CertificateRenewalHistoryModel> GetCertificateRenewalHistoryAsync(string certificateId)
        {
            if (string.IsNullOrEmpty(certificateId))
                throw new ArgumentException("Certificate ID is required");

            try
            {
                var currentCertificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
                if (currentCertificate == null)
                {
                    throw new KeyNotFoundException($"Certificate with ID {certificateId} not found");
                }

                if (currentCertificate.Status != CertificateStatus.Active &&
                    currentCertificate.Status != CertificateStatus.Pending)
                {
                    _logger.LogWarning($"Certificate with ID {certificateId} is not active or pending (Status: {currentCertificate.Status})");
                }

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(currentCertificate.CourseId);
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course with ID {currentCertificate.CourseId} not found");
                }

                var isRecurrentCourse = course.CourseLevel == CourseLevel.Recurrent;
                var originalCourseId = isRecurrentCourse ? course.RelatedCourseId : course.CourseId;

                if (isRecurrentCourse && string.IsNullOrEmpty(originalCourseId))
                {
                    _logger.LogWarning($"Recurrent course {course.CourseId} has no related initial course specified");
                }

                var relatedCertificates = new List<Certificate>();
                var currentCourseCerts = await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                    c.UserId == currentCertificate.UserId &&
                    c.CourseId == currentCertificate.CourseId &&
                    c.Status != CertificateStatus.Revoked);
                relatedCertificates.AddRange(currentCourseCerts);

                if (isRecurrentCourse && !string.IsNullOrEmpty(originalCourseId))
                {
                    var initialCourseCerts = await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                        c.UserId == currentCertificate.UserId &&
                        c.CourseId == originalCourseId &&
                        c.Status != CertificateStatus.Revoked);
                    relatedCertificates.AddRange(initialCourseCerts);
                }
                else if (!isRecurrentCourse)
                {
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

                relatedCertificates = relatedCertificates.Distinct().ToList();
                var orderedCertificates = relatedCertificates
                    .OrderBy(c => c.IssueDate)
                    .ToList();

                if (!orderedCertificates.Any())
                {
                    _logger.LogWarning($"No certificates found for user {currentCertificate.UserId} related to course {course.CourseId}");
                    if (currentCertificate.Status != CertificateStatus.Revoked)
                    {
                        orderedCertificates.Add(currentCertificate);
                    }
                    else
                    {
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

                var originalCertificate = orderedCertificates.First();
                var allUserIds = orderedCertificates
                    .Select(c => c.IssueByUserId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                var users = await _userRepository.GetUsersByIdsAsync(allUserIds);
                var userDict = users.ToDictionary(u => u.UserId);

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

                for (int i = 1; i < orderedCertificates.Count; i++)
                {
                    var currentCert = orderedCertificates[i];
                    var previousCert = orderedCertificates[i - 1];

                    bool isRenewal = false;

                    if (currentCert.CourseId != previousCert.CourseId)
                    {
                        var currentCertCourse = await _unitOfWork.CourseRepository.GetByIdAsync(currentCert.CourseId);
                        if (currentCertCourse != null && currentCertCourse.CourseLevel == CourseLevel.Recurrent)
                        {
                            isRenewal = true;
                        }
                    }
                    else
                    {
                        var previousExpiryDate = previousCert.ExpirationDate ?? previousCert.IssueDate.AddYears(2);
                        var timeDiff = currentCert.IssueDate - previousCert.IssueDate;
                        isRenewal = timeDiff.TotalDays > 365 ||
                                   (previousExpiryDate - currentCert.IssueDate).TotalDays <= 180;
                    }

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
        public async Task<List<CertificateRenewalHistoryModel>> GetUserCertificateRenewalHistoryAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required");

            try
            {
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
                _memoryCache.Set(cacheKey, templateHtml, TimeSpan.FromMinutes(TEMPLATE_CACHE_MINUTES));
            }

            return templateHtml;
        }

        private async Task<string> GetTemplateHtmlFromBlobAsync(string templateFileUrl)
        {
            try
            {
                Uri blobUri = new Uri(templateFileUrl);
                string accountUrl = $"{blobUri.Scheme}://{blobUri.Host}";
                string containerName = blobUri.Segments[1].TrimEnd('/');
                string blobName = blobUri.AbsolutePath.Substring(blobUri.AbsolutePath.IndexOf(containerName) + containerName.Length + 1);

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

                var matchingTemplates = new List<CertificateTemplate>();

                switch (courseLevel)
                {
                    case CourseLevel.Initial:
                        matchingTemplates = templates.Where(t => t.TemplateName.Contains("Initial")).ToList();
                        break;
                    case CourseLevel.Relearn:
                        matchingTemplates = templates.Where(t => t.TemplateName.Contains("Initial")).ToList();
                        break;
                    case CourseLevel.Professional:
                        matchingTemplates = templates.Where(t => t.TemplateName.Contains("Professional")).ToList();
                        break;
                    default:
                        matchingTemplates = templates.ToList();
                        break;
                }

                if (!matchingTemplates.Any())
                    return null;

                templateId = matchingTemplates
                    .OrderByDescending(t =>
                    {
                        var parts = t.CertificateTemplateId.Split('-');
                        if (parts.Length >= 3 && int.TryParse(parts[2], out int sequenceNumber))
                            return sequenceNumber;
                        return 0;
                    })
                    .FirstOrDefault()?.CertificateTemplateId;

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

        private string GenerateCertificateCode(Course course, User trainee)
        {
            string courseLevel = course.CourseLevel.ToString().Substring(0, 3).ToUpper();
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString("D2");

            string hash = (trainee.UserId + course.CourseId).GetHashCode().ToString("X").Substring(0, 5);

            return $"OCMS-{courseLevel}-{year}-{month}-{hash}";
        }

        private async Task<string> SaveCertificateToBlob(string htmlContent, string fileName)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
            var blobUrl = await _blobService.UploadFileAsync("certificates", fileName, stream, "text/html");
            return _blobService.GetBlobUrlWithoutSasToken(blobUrl);
        }
        private async Task<Certificate> GenerateCertificateAsync(
            User trainee,
            Course course,
            string templateId,
            string templateHtml,
            string templateType,
            List<Grade> grades,
            string issuedByUserId,
            DateTime issueDate,
            string specialtyId)
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
                SignDate = DateTime.Now,
                SpecialtyId = specialtyId // Set SpecialtyId here
            };
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
            var startDate = issueDate.ToString("dd/MM/yyyy");
            var endDate = issueDate.AddYears(3).ToString("dd/MM/yyyy");
            double averageScore = grades.Average(g => g.TotalScore);
            string gradeResult = DetermineGradeResult(averageScore);
            string avatarBase64 = await GetBase64AvatarAsync(trainee.AvatarUrl);

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

            if (templateType == "Initial")
            {
                result = result.Replace("[LOẠI TỐT NGHIỆP]", gradeResult);
            }

            var currentDate = DateTime.Now;
            result = Regex.Replace(result,
                @"ngày\s+\d+\s+tháng\s+\d+\s+năm\s+\d+",
                $"ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}");

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
                string cacheKey = $"avatar_base64_{avatarUrl.GetHashCode()}";
                if (_memoryCache.TryGetValue(cacheKey, out string cachedAvatar))
                {
                    return cachedAvatar;
                }

                var urlWithSasToken = await _blobService.GetBlobUrlWithSasTokenAsync(avatarUrl, TimeSpan.FromMinutes(5));

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(urlWithSasToken);
                    response.EnsureSuccessStatusCode();

                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    string base64String = Convert.ToBase64String(bytes);
                    string result = $"data:{contentType};base64,{base64String}";

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

            string defaultAvatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "default-avatar.jpg");

            if (File.Exists(defaultAvatarPath))
            {
                byte[] bytes = await File.ReadAllBytesAsync(defaultAvatarPath);
                string base64String = Convert.ToBase64String(bytes);
                string result = $"data:image/jpeg;base64,{base64String}";

                _memoryCache.Set(cacheKey, result, TimeSpan.FromDays(365));

                return result;
            }

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