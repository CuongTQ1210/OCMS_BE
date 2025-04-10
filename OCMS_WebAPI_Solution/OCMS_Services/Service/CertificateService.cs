using AutoMapper;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
                Course = course,
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

        #region Delete Certificate
        /// <summary>
        /// Deletes a certificate by its ID and removes the associated file from Blob storage
        /// </summary>
        /// <param name="certificateId">The ID of the certificate to delete</param>
        /// <returns>True if successfully deleted, otherwise false</returns>
        public async Task<bool> DeleteCertificateAsync(string certificateId)
        {
            if (string.IsNullOrEmpty(certificateId))
            {
                throw new ArgumentException("CertificateId is required");
            }

            bool result = false;

            await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                // Start a transaction to ensure consistency
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Get the certificate to be deleted
                    var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
                    if (certificate == null)
                    {
                        throw new KeyNotFoundException($"Certificate with ID {certificateId} not found");
                    }

                    // Store the certificate file URL for deletion
                    string certificateFileURL = certificate.CertificateURL;

                    // Delete the certificate record from the database
                    await _unitOfWork.CertificateRepository.DeleteAsync(certificateId);
                    await _unitOfWork.SaveChangesAsync();

                    // Delete the certificate file from Blob storage if it exists
                    if (!string.IsNullOrEmpty(certificateFileURL))
                    {
                        await _blobService.DeleteFileAsync(certificateFileURL);
                        _logger.LogInformation($"Certificate file deleted from blob storage: {certificateFileURL}");
                    }

                    // Commit the transaction if everything is successful
                    await _unitOfWork.CommitTransactionAsync();
                    result = true;
                    _logger.LogInformation($"Certificate with ID {certificateId} successfully deleted");
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if an error occurs
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, $"Error deleting certificate with ID {certificateId}");
                    throw new Exception($"Error deleting certificate", ex);
                }
            });

            return result;
        }
        #endregion

        #region Helper Methods       
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
                    return templates.FirstOrDefault()?.CertificateTemplateId;
            }

            if (!matchingTemplates.Any())
                return null;

            // If multiple matching templates exist, select the one with the highest sequence number
            // Template IDs follow the format: TEMP-XXX-NNN where XXX is the type and NNN is the sequence
            return matchingTemplates
                .OrderByDescending(t => {
                    // Extract the sequence number (last 3 digits after last dash)
                    var parts = t.CertificateTemplateId.Split('-');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int sequenceNumber))
                        return sequenceNumber;
                    return 0;
                })
                .FirstOrDefault()?.CertificateTemplateId;
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
            var startDate = issueDate.ToString("dd/MM/yyyy");
            var endDate = issueDate.AddYears(3).ToString("dd/MM/yyyy");
            double averageScore = grades.Average(g => g.TotalScore);
            string gradeResult = DetermineGradeResult(averageScore); // Hàm giả định để xác định loại tốt nghiệp
            string avatarBase64 = await GetBase64AvatarAsync(trainee.AvatarUrl);


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
                // Parse URL để lấy container name và blob name
                Uri blobUri = new Uri(avatarUrl);
                string accountUrl = $"{blobUri.Scheme}://{blobUri.Host}";
                string containerName = blobUri.Segments[1].TrimEnd('/');
                string blobName = blobUri.AbsolutePath.Substring(blobUri.AbsolutePath.IndexOf(containerName) + containerName.Length + 1);

                // Tạo BlobServiceClient
                BlobServiceClient blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new DefaultAzureCredential());

                // Lấy container client và blob client
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Tải blob content
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                // Đọc nội dung vào memory stream
                using var memoryStream = new MemoryStream();
                await download.Content.CopyToAsync(memoryStream);

                // Chuyển đổi thành Base64
                byte[] bytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(bytes);

                // Xác định Content-Type của file (hoặc bạn có thể lưu Content-Type của avatar khi upload)
                string contentType = download.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = "image/jpeg"; // Mặc định là JPEG
                }

                // Trả về đường dẫn Data URL có thể dùng trong src của thẻ img
                return $"data:{contentType};base64,{base64String}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating base64 for avatar");
                return await GetDefaultBase64AvatarAsync();
            }
        }

        private async Task<string> GetDefaultBase64AvatarAsync()
        {
            // Tạo một avatar mặc định dạng Base64
            string defaultAvatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "default-avatar.jpg");

            if (File.Exists(defaultAvatarPath))
            {
                byte[] bytes = await File.ReadAllBytesAsync(defaultAvatarPath);
                string base64String = Convert.ToBase64String(bytes);
                return $"data:image/jpeg;base64,{base64String}";
            }

            // Nếu không có file mặc định, trả về một ảnh đơn giản dạng Base64
            // Đây là một ảnh đơn giản màu xám với biểu tượng người dùng
            return "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDAiIGhlaWdodD0iMTAwIiB2aWV3Qm94PSIwIDAgMTAwIDEwMCI+PHJlY3Qgd2lkdGg9IjEwMCIgaGVpZ2h0PSIxMDAiIGZpbGw9IiNlMGUwZTAiLz48Y2lyY2xlIGN4PSI1MCIgY3k9IjM1IiByPSIyMCIgZmlsbD0iIzlFOUU5RSIvPjxwYXRoIGQ9Ik0yNSw4NSBDMjUsNjAgNzUsNjAgNzUsODUgWiIgZmlsbD0iIzlFOUU5RSIvPjwvc3ZnPg==";
        }
        #endregion
    }
}