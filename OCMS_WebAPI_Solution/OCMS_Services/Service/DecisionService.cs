using AutoMapper;
using Microsoft.Extensions.Logging;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
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
    public class DecisionService : IDecisionService
    {
        private readonly IDecisionTemplateService _templateService;
        private readonly IPdfSignerService _pdfSignerService;
        private readonly IBlobService _blobService;
        private readonly UnitOfWork _unitOfWork;
        private readonly ILogger<DecisionService> _logger;
        private readonly IDecisionRepository _decisionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public DecisionService(
            IDecisionTemplateService templateService,
            IPdfSignerService pdfSignerService,
            IBlobService blobService,
            UnitOfWork unitOfWork,
            ILogger<DecisionService> logger,
            IDecisionRepository decisionRepository,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            INotificationService notificationService,
            IMapper mapper)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _pdfSignerService = pdfSignerService ?? throw new ArgumentNullException(nameof(pdfSignerService));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _decisionRepository = decisionRepository ?? throw new ArgumentNullException(nameof(decisionRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _mapper = mapper;
        }

        #region Create Decision
        public async Task<CreateDecisionResponse> CreateDecisionForCourseAsync(CreateDecisionDTO request, string issuedByUserId)
        {
            // 1. Lấy khóa học và chứng chỉ đã được phê duyệt
            var course = await _courseRepository.GetCourseWithDetailsAsync(request.CourseId);
            if (course == null)
                throw new InvalidOperationException("Course not found");

            if (course.Progress != Progress.Completed)
                throw new InvalidOperationException("Course has not been completed yet");

            var approvedCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                c => c.CourseId == request.CourseId && c.Status == CertificateStatus.Active);

            if (!approvedCertificates.Any())
                throw new InvalidOperationException("No approved certificates found for this course");

            // 2. Xác định template dựa trên CourseLevel
            string templateNamePrefix;
            switch (course.CourseLevel)
            {
                case CourseLevel.Initial:
                    templateNamePrefix = "Initial";
                    break;
                case CourseLevel.Recurrent:
                    templateNamePrefix = "Recurrent";
                    break;
                case CourseLevel.Relearn:
                    templateNamePrefix = "Initial";
                    break;
                default:
                    templateNamePrefix = "Initial";
                    break;
            }

            // Tìm template phù hợp nhất theo tên
            var decisionTemplate = await _unitOfWork.DecisionTemplateRepository.GetAllAsync(
                dt => dt.TemplateName.StartsWith(templateNamePrefix) && dt.TemplateStatus == 1);

            // Lấy template mới nhất (giả sử CreatedAt là thời gian tạo)
            var latestTemplate = decisionTemplate
                .OrderByDescending(dt => dt.CreatedAt)
                .FirstOrDefault();

            // Nếu không tìm thấy template cụ thể, sử dụng fallback strategy
            if (latestTemplate == null && templateNamePrefix == "Relearn")
            {
                // Fallback: sử dụng template Recurrent cho Relearn nếu không có template riêng
                latestTemplate = (await _unitOfWork.DecisionTemplateRepository.GetAllAsync(
                    dt => dt.TemplateName.StartsWith("Recurrent") && dt.TemplateStatus == 1))
                    .OrderByDescending(dt => dt.CreatedAt)
                    .FirstOrDefault();
            }

            if (latestTemplate == null)
                throw new InvalidOperationException($"No active decision template found for {course.CourseLevel} course level");

            // 3. Lấy template HTML từ blob
            string templateHtml = "";
            if (latestTemplate.TemplateContent.StartsWith("https"))
            {
                var sasUrl = await _blobService.GetBlobUrlWithSasTokenAsync(latestTemplate.TemplateContent, TimeSpan.FromHours(1), "r");
                using (var httpClient = new HttpClient())
                {
                    templateHtml = await httpClient.GetStringAsync(sasUrl);
                }
            }
            else
            {
                templateHtml = latestTemplate.TemplateContent;
            }

            // 4. Chuẩn bị dữ liệu
            var decisionCode = GenerateDecisionCode();
            var issueDate = DateTime.UtcNow;
            var studentRows = await GenerateStudentRowsAsync(approvedCertificates);
            var courseSchedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(ts => ts.Subject.CourseId == request.CourseId);
            var startDate = courseSchedules.Any() ? courseSchedules.Min(s => s.StartDateTime) : issueDate;
            var endDate = courseSchedules.Any() ? courseSchedules.Max(s => s.EndDateTime) : issueDate;

            // 5. Điền dữ liệu vào template
            string decisionContent = templateHtml
                .Replace("{{DecisionCode}}", decisionCode)
                .Replace("{{Day}}", issueDate.Day.ToString())
                .Replace("{{Month}}", issueDate.Month.ToString())
                .Replace("{{Year}}", issueDate.Year.ToString())
                .Replace("{{CourseCode}}", course.CourseName)
                .Replace("{{CourseTitle}}", course.CourseName ?? $"Khóa {course.CourseName}")
                .Replace("{{StudentCount}}", approvedCertificates.Count().ToString())
                .Replace("{{StartDate}}", startDate.ToString("dd/MM/yyyy"))
                .Replace("{{EndDate}}", endDate.ToString("dd/MM/yyyy"))
                .Replace("{{StudentRows}}", studentRows);

            // 6. Lưu Quyết định vào blob
            string blobName = $"decision_{decisionCode}_{DateTime.UtcNow:yyyyMMddHHmmss}.html";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(decisionContent));
            var blobUrl = await _blobService.UploadFileAsync("decisions", blobName, stream, "text/html");
            var blobUrlWithoutSas = _blobService.GetBlobUrlWithoutSasToken(blobUrl);
            var primaryCertificate = approvedCertificates.FirstOrDefault();
            if (primaryCertificate == null)
                throw new InvalidOperationException("No certificate found to associate with the decision");

            // 7. Tạo entity Decision
            var decision = new Decision
            {
                DecisionId = Guid.NewGuid().ToString(),
                DecisionCode = decisionCode,
                Title = $"Quyết định cho khóa học {course.CourseName}",
                Content = blobUrlWithoutSas,
                IssueDate = issueDate,
                IssuedByUserId = issuedByUserId,
                DecisionTemplateId = latestTemplate.DecisionTemplateId,
                DecisionStatus = 0, // Draft
                CertificateId = primaryCertificate.CertificateId
            };

            // 8. Lưu vào database
            await _unitOfWork.DecisionRepository.AddAsync(decision);
            await _unitOfWork.SaveChangesAsync();

            // 9. Thông báo cho HeadMaster
            await NotifyHeadMasterForSignatureAsync(decision.DecisionId, course.CourseName);

            // 10. Map to response
            return _mapper.Map<CreateDecisionResponse>(decision);
        }
        #endregion

        #region Helper Methods
        private string GenerateDecisionCode()
        {
            return $"QD-{DateTime.Now.Year}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }

        private async Task<string> GenerateStudentRowsAsync(IEnumerable<Certificate> certificates)
        {
            var studentRows = new StringBuilder();
            int index = 1;
            foreach (var cert in certificates)
            {
                var trainee = await _userRepository.GetByIdAsync(cert.UserId);
                if (trainee != null)
                {
                    string department = trainee.Specialty?.SpecialtyName ?? "Chưa xác định";
                    studentRows.AppendLine("<tr>");
                    studentRows.AppendLine($"  <td>{index}</td>");
                    studentRows.AppendLine($"  <td>{trainee.FullName}</td>");
                    studentRows.AppendLine($"  <td>{trainee.Username}</td>");
                    studentRows.AppendLine($"  <td>{department}</td>");
                    // Thêm cột Ghi chú cho Recurrent_Decision nếu cần
                    if (certificates.First().Course.CourseLevel != CourseLevel.Initial)
                        studentRows.AppendLine("  <td></td>");
                    studentRows.AppendLine("</tr>");
                    index++;
                }
            }
            return studentRows.ToString();
        }

        private async Task NotifyHeadMasterForSignatureAsync(string decisionId, string courseName)
        {
            var headMasters = await _userRepository.GetUsersByRoleAsync("HeadMaster");
            foreach (var hm in headMasters)
            {
                await _notificationService.SendNotificationAsync(
                    hm.UserId,
                    "Yêu cầu ký số Quyết định",
                    $"Một Quyết định cho khóa học '{courseName}' cần được ký số. Vui lòng xem xét và ký.",
                    "DecisionSignature"
                );
            }
        }
        #endregion
    }
}