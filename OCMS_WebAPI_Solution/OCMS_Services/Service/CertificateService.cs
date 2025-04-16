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
                if (course == null || !course.Subjects.Any())
                {
                    _logger.LogWarning($"Course with ID {courseId} not found or has no subjects");
                    return createdCertificates;
                }

                int subjectCount = course.Subjects.Count;
                _logger.LogInformation($"Course {courseId} has {subjectCount} subjects");

                // 2. Get template data with caching
                var templateId = await GetTemplateIdByCourseLevelAsync(course.CourseLevel);
                if (string.IsNullOrEmpty(templateId))
                {
                    _logger.LogWarning($"No active template for course level {course.CourseLevel}");
                    return createdCertificates;
                }

                var certificateTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(templateId);
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
                var generationTasks = eligibleTrainees.Select(async ta =>
                {
                    if (!traineeDict.TryGetValue(ta.TraineeId, out var trainee))
                        return null;

                    try
                    {
                        var grades = gradesByTraineeAssign[ta.TraineeAssignId];
                        return await GenerateCertificateAsync(
                            trainee, course, templateId, templateHtml, templateType, grades, issuedByUserId, issueDate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to create certificate for trainee {ta.TraineeId}");
                        return null;
                    }
                });

                var certificates = (await Task.WhenAll(generationTasks))
                    .Where(c => c != null)
                    .ToList();

                certToCreate.AddRange(certificates);

                // 9. Save to database with transaction
                if (certToCreate.Any())
                {
                    _logger.LogInformation($"Saving {certToCreate.Count} new certificates to database");

                    await _unitOfWork.ExecuteWithStrategyAsync(async () =>
                    {
                        await _unitOfWork.BeginTransactionAsync();
                        try
                        {
                            await _unitOfWork.CertificateRepository.AddRangeAsync(certToCreate);
                            await _unitOfWork.SaveChangesAsync();
                            await _unitOfWork.CommitTransactionAsync();

                            createdCertificates = _mapper.Map<List<CertificateModel>>(certToCreate);

                            // 10. Notify HeadMasters efficiently
                            await NotifyTrainingStaffsAsync(createdCertificates.Count, course.CourseName);

                            _logger.LogInformation($"Successfully created {createdCertificates.Count} certificates for course {courseId}");
                        }
                        catch (Exception ex)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            _logger.LogError(ex, "Transaction failed during certificate creation");
                            throw;
                        }
                    });
                }

                return createdCertificates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AutoGenerateCertificatesForPassedTraineesAsync for course {courseId}");
                throw new Exception("Failed to auto-generate certificates", ex);
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

                // Filter templates based on course level
                var matchingTemplates = new List<CertificateTemplate>();

                switch (courseLevel)
                {
                    case CourseLevel.Initial:
                        matchingTemplates = templates.Where(t => t.TemplateName.Contains("Initial")).ToList();
                        break;
                    case CourseLevel.Recurrent:
                        matchingTemplates = templates.Where(t => t.TemplateName.Contains("Recurrent")).ToList();
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
                    .OrderByDescending(t => {
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
            if (templateName.Contains("Recurrent")) return "Recurrent";
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
            return new Certificate
            {
                CertificateId = Guid.NewGuid().ToString(),
                CertificateCode = certificateCode,
                UserId = trainee.UserId,
                CourseId = course.CourseId,
                CertificateTemplateId = templateId,
                IssueByUserId = issuedByUserId,
                IssueDate = issueDate,
                Status = CertificateStatus.Pending,
                CertificateURL = certificateUrl,
                IsRevoked = false,
                Course= courseExist,
                User=userExist,
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
                .Replace("[MÃ QUYẾT ĐỊNH]", $"ATO-{DateTime.UtcNow.Year}-{course.CourseId.Substring(0, 4)}")
                .Replace("[Mã quyết định]", $"ATO-{DateTime.UtcNow.Year}-{course.CourseId.Substring(0, 4)}")
                .Replace("[MÃ CHỨNG CHỈ]", certificateCode)
                .Replace("[Mã chứng chỉ]", certificateCode);

            // Replace template-specific placeholders
            if (templateType == "Initial")
            {
                result = result.Replace("[LOẠI TỐT NGHIỆP]", gradeResult);
            }

            // Update date in signature
            var currentDate = DateTime.UtcNow;
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

            return _mapper.Map<List<CertificateModel>>(certificates);
        }

        private async Task<List<CertificateModel>> GetAllPendingCertificatesAsync()
        {
            var pendingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.Status ==CertificateStatus.Pending);
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