using AutoMapper;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Repositories.Repository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class CertificateService : ICertificateService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CertificateService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITraineeAssignRepository _traineeAssignRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly ICertiTempRepository _certificateTemplateRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly IMapper _mapper;

        public CertificateService(
            UnitOfWork unitOfWork,
            IBlobService blobService,
            INotificationService notificationService,
            ILogger<CertificateService> logger,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            ITraineeAssignRepository traineeAssignRepository,
            IGradeRepository gradeRepository,
            ICertiTempRepository certificateTemplateRepository,
            IRequestRepository requestRepository,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _traineeAssignRepository = traineeAssignRepository ?? throw new ArgumentNullException(nameof(traineeAssignRepository));
            _gradeRepository = gradeRepository ?? throw new ArgumentNullException(nameof(gradeRepository));
            _certificateTemplateRepository = certificateTemplateRepository ?? throw new ArgumentNullException(nameof(certificateTemplateRepository));
            _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
            _mapper = mapper;
        }

        #region Create Certificate For Single Trainee
        /// <summary>
        /// Creates a certificate for a specific trainee who has completed a course
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <param name="traineeId">The ID of the trainee</param>
        /// <param name="issuedByUserId">The ID of the user issuing the certificate</param>
        /// <returns>Certificate DTO of the created certificate</returns>
        public async Task<CertificateModel> CreateCertificateAsync(CreateCertificateDTO request, string issuedByUserId)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(request.CourseId) || string.IsNullOrEmpty(request.TraineeId) || string.IsNullOrEmpty(issuedByUserId))
            {
                throw new ArgumentException("CourseId, TraineeId, và IssuedByUserId là bắt buộc.");
            }

            // Kiểm tra xem chứng chỉ đã tồn tại chưa
            var existingCertificate = await _unitOfWork.CertificateRepository.GetAsync(
                c => c.UserId == request.TraineeId && c.CourseId == request.CourseId);
            if (existingCertificate != null)
            {
                throw new InvalidOperationException("Chứng chỉ đã tồn tại cho Trainee và Course này.");
            }

            // Lấy thông tin TraineeAssign để xác nhận Trainee tham gia khóa học
            var traineeAssign = await _traineeAssignRepository.GetTraineeAssignmentAsync(request.CourseId, request.TraineeId);
            if (traineeAssign == null)
            {
                throw new ArgumentException("Trainee không được assign vào khóa học này.");
            }

            // Lấy tất cả subjects của khóa học
            var course = await _courseRepository.GetCourseWithDetailsAsync(request.CourseId);
            if (course == null)
            {
                throw new ArgumentException("Không tìm thấy khóa học.");
            }

            // Lấy grades của Trainee
            var grades = await _gradeRepository.GetGradesByTraineeAssignIdAsync(traineeAssign.TraineeAssignId);

            // Kiểm tra Trainee đã Pass tất cả subjects chưa
            if (course.Subjects.Count != grades.Count() || !grades.All(g => g.gradeStatus == GradeStatus.Pass))
            {
                throw new InvalidOperationException("Trainee chưa hoàn thành hoặc chưa Pass tất cả subjects trong khóa học.");
            }

            // Chọn template phù hợp với loại khóa học
            var templateId = await GetTemplateIdByCourseLevelAsync(course.CourseLevel);
            if (string.IsNullOrEmpty(templateId))
            {
                throw new InvalidOperationException("Không tìm thấy template chứng chỉ phù hợp.");
            }

            var certificateTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(templateId);

            // Lấy HTML template từ Blob Storage
            string templateHtml = await GetTemplateHtmlFromBlobAsync(certificateTemplate.TemplateFile);

            // Lấy thông tin Trainee
            var trainee = await _unitOfWork.UserRepository.GetByIdAsync(request.TraineeId);
            if (trainee == null)
            {
                throw new ArgumentException("Không tìm thấy Trainee.");
            }

            // Tạo mã chứng chỉ và ngày phát hành
            var issueDate = DateTime.UtcNow;
            string certificateCode = GenerateCertificateCode(course, trainee);

            // Thay thế dữ liệu vào template
            string templateType = GetTemplateTypeFromName(certificateTemplate.TemplateName);
            var modifiedHtml = await PopulateTemplateAsync(templateHtml, trainee, course, issueDate, certificateCode, grades, templateType);

            // Lưu file chứng chỉ vào Blob storage
            string certificateFileName = $"certificate_{certificateCode}_{DateTime.UtcNow:yyyyMMddHHmmss}.html";
            string certificateUrl = await SaveCertificateToBlob(modifiedHtml, certificateFileName);

            // Tạo entity Certificate
            var certificate = new Certificate
            {
                CertificateId = Guid.NewGuid().ToString(),
                CertificateCode = certificateCode,
                UserId = request.TraineeId,
                CourseId = request.CourseId,
                CertificateTemplateId = templateId,
                IssueByUserId = issuedByUserId,
                IssueDate = issueDate,
                Status = CertificateStatus.Active,
                CertificateURL = certificateUrl,
                IsRevoked = false,
                SignDate = DateTime.UtcNow
            };

            // Thêm certificate mới
            await _unitOfWork.CertificateRepository.AddAsync(certificate);

            // Gửi request ký cho Director
            await CreateSignatureRequestAsync(certificate.CertificateId, issuedByUserId);

            // Lưu thay đổi
            await _unitOfWork.SaveChangesAsync();

            // Ánh xạ và trả về response
            return _mapper.Map<CertificateModel>(certificate);
        }
        #endregion

        #region Get Trainee Certificates
        public async Task<List<CertificateModel>> GetCertificatesByTraineeIdAsync(string traineeId)
        {
            if (string.IsNullOrEmpty(traineeId))
            {
                throw new ArgumentException("TraineeId is required");
            }

            // Kiểm tra trainee có tồn tại không
            var trainee = await _unitOfWork.UserRepository.GetByIdAsync(traineeId);
            if (trainee == null)
            {
                throw new ArgumentException($"Trainee with ID {traineeId} not found");
            }

            // Lấy tất cả certificate của trainee
            var certificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                c => c.UserId == traineeId,
                c => c.Course,
                c => c.CertificateTemplate,
                c => c.IssueByUser
            );

            // Ánh xạ các certificate thành certificate models
            return _mapper.Map<List<CertificateModel>>(certificates);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Creates a signature request for directors to sign a certificate
        /// </summary>
        /// <param name="certificateId">The ID of the certificate to be signed</param>
        /// <param name="requestedByUserId">The ID of the user requesting the signature</param>
        /// <returns>Request ID of the created signature request</returns>
        private async Task<string> CreateSignatureRequestAsync(string certificateId, string requestedByUserId)
        {
            try
            {
                // Get certificate details for notification message
                var certificate = await _unitOfWork.CertificateRepository.GetAsync(
                    c => c.CertificateId == certificateId,
                    c => c.User, c => c.Course);

                if (certificate == null)
                {
                    throw new ArgumentException($"Certificate with ID {certificateId} not found");
                }

                // Create signature request
                var request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    RequestUserId = requestedByUserId,
                    RequestEntityId = certificateId,
                    RequestType = RequestType.CreateNew, // Use appropriate request type for certificate signing
                    RequestDate = DateTime.UtcNow,
                    Description = $"Certificate signature request for {certificate.User.FullName} - Course: {certificate.Course.CourseName}",
                    Notes = "Please review and sign this certificate",
                    Status = RequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.RequestRepository.AddAsync(request);

                // Find all directors to notify for signing
                var directors = await _userRepository.GetUsersByRoleAsync("Director");

                // Send notifications to all directors
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "Certificate Pending Signature",
                        $"A new certificate has been created for {certificate.User.FullName} for the course '{certificate.Course.CourseName}' and is pending your signature.",
                        "Certificate"
                    );
                }

                _logger.LogInformation($"Signature request created for certificate {certificateId}");

                return request.RequestId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating signature request for certificate {certificateId}");
                throw;
            }
        }

        private string GenerateCertificateCode(Course course, User trainee)
        {
            // Create a unique code format like: VJA-{CourseLevel}-{Year}-{Month}-{Sequential Number}
            string courseLevel = course.CourseLevel.ToString().Substring(0, 3).ToUpper();
            string year = DateTime.UtcNow.Year.ToString();
            string month = DateTime.UtcNow.Month.ToString("D2");

            // Generate a hash from trainee ID + course ID
            string hash = (trainee.UserId + course.CourseId).GetHashCode().ToString("X").Substring(0, 5);

            return $"OCMS-{courseLevel}-{year}-{month}-{hash}";
        }

        private async Task<string> GetTemplateIdByCourseLevelAsync(CourseLevel courseLevel)
        {
            var templates = await _unitOfWork.CertificateTemplateRepository.GetAllAsync(
                t => t.templateStatus == TemplateStatus.Active);

            string templateId = null;
            switch (courseLevel)
            {
                case CourseLevel.Initial:
                    templateId = templates.FirstOrDefault(t => t.TemplateName.Contains("Initial"))?.CertificateTemplateId;
                    break;
                case CourseLevel.Recurrent:
                    templateId = templates.FirstOrDefault(t => t.TemplateName.Contains("Recurrent"))?.CertificateTemplateId;
                    break;
                case CourseLevel.Relearn:
                    templateId = templates.FirstOrDefault(t => t.TemplateName.Contains("Professional"))?.CertificateTemplateId;
                    break;
                default:
                    templateId = templates.FirstOrDefault()?.CertificateTemplateId;
                    break;
            }

            return templateId;
        }

        private async Task<string> GetTemplateHtmlFromBlobAsync(string templateFileUrl)
        {
            // Phân tích URL để lấy endpoint của account
            Uri blobUri = new Uri(templateFileUrl);
            string accountUrl = $"{blobUri.Scheme}://{blobUri.Host}";
            string containerName = blobUri.Segments[1].TrimEnd('/');
            string blobName = blobUri.AbsolutePath.Substring(blobUri.AbsolutePath.IndexOf(containerName) + containerName.Length + 1);

            // Sử dụng DefaultAzureCredential (sẽ sử dụng Managed Identity khi chạy trên Azure)
            BlobServiceClient blobServiceClient = new BlobServiceClient(
                new Uri(accountUrl),
                new DefaultAzureCredential());
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;

            using var reader = new StreamReader(memoryStream);
            return await reader.ReadToEndAsync();
        }

        private string GetTemplateTypeFromName(string templateName)
        {
            if (templateName.Contains("Initial")) return "Initial";
            if (templateName.Contains("Recurrent")) return "Recurrent";
            if (templateName.Contains("Professional")) return "Professional";
            return "Standard";
        }

        private async Task<string> SaveCertificateToBlob(string htmlContent, string fileName)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
            return await _blobService.UploadFileAsync("certificates", fileName, stream);
        }

        private async Task<string> PopulateTemplateAsync(string templateHtml, User trainee, Course course, DateTime issueDate, string certificateCode, IEnumerable<Grade> grades, string templateType)
        {
            // Lấy thông tin chung
            var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(s => s.Subject.CourseId == course.CourseId);
            var startDate = schedules.Min(s => s.StartDateTime).ToString("dd/MM/yyyy");
            var endDate = schedules.Max(s => s.EndDateTime).ToString("dd/MM/yyyy");
            double averageScore = grades.Average(g => g.TotalScore);
            string gradeResult = DetermineGradeResult(averageScore); // Hàm giả định để xác định loại tốt nghiệp

            // Thay thế các placeholder chung
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

            // Thay thế placeholder đặc biệt theo loại template
            if (templateType == "Initial")
            {
                result = result.Replace("[LOẠI TỐT NGHIỆP]", gradeResult);
            }
            else if (templateType == "Professional")
            {
                // Professional có field "Tốt nghiệp loại" cố định là "Đạt / Pass" trong template
                // Nếu cần tính toán dựa trên điểm, bạn có thể thay đổi logic ở đây
            }
            // Recurrent không có placeholder đặc biệt nào khác

            // Cập nhật ngày tháng trong chữ ký
            var currentDate = DateTime.UtcNow;
            result = Regex.Replace(result,
                @"ngày\s+\d+\s+tháng\s+\d+\s+năm\s+\d+",
                $"ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}");

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
        #endregion
    }
}