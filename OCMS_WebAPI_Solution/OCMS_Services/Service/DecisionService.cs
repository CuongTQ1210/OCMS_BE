using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class DecisionService : IDecisionService
    {
        private readonly IBlobService _blobService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly Lazy<IDecisionTemplateService> _decisionTemplateService;
        private readonly Lazy<IRequestService> _requestService;

        public DecisionService(
            Lazy<IDecisionTemplateService> decisionTemplateService,
            IBlobService blobService,
            UnitOfWork unitOfWork,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            INotificationService notificationService,
            IMapper mapper,
            Lazy<IRequestService> requestService)
        {
            _decisionTemplateService = decisionTemplateService ?? throw new ArgumentNullException(nameof(decisionTemplateService));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
        }

        #region Create Decision
        public async Task<CreateDecisionResponse> CreateDecisionForCourseAsync(CreateDecisionDTO request, string issuedByUserId)
        {
            if (string.IsNullOrEmpty(request?.CourseId))
                throw new ArgumentException("CourseId is required", nameof(request));

            if (string.IsNullOrEmpty(issuedByUserId))
                throw new ArgumentException("IssuedByUserId is required", nameof(issuedByUserId));

            // Get the execution strategy from the DbContext
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();

            // Define the operation to execute
            return await strategy.ExecuteAsync(
                state: _unitOfWork.Context, // Pass the DbContext as the state
                operation: async (dbContext, ctx, cancellationToken) =>
                {
                    // 1. Lấy khóa học và chứng chỉ đã được phê duyệt
                    var course = await _courseRepository.GetCourseWithDetailsAsync(request.CourseId);
                    if (course == null || course.Status != CourseStatus.Approved)
                        throw new InvalidOperationException("Course not found or not active");

                    // Begin a transaction
                    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        IEnumerable<Certificate> certificates = new List<Certificate>();
                        IEnumerable<TraineeAssign> traineeAssigns = await _unitOfWork.TraineeAssignRepository
                            .GetAllAsync(t => t.ClassSubject.Class.CourseId == request.CourseId && t.RequestStatus == RequestStatus.Approved);

                        if (!traineeAssigns.Any())
                            throw new InvalidOperationException("No approved trainees found for this course");

                        var matchedCertificates = new List<Certificate>();

                        foreach (var trainee in traineeAssigns)
                        {
                            Certificate? cert;

                            if (course.CourseLevel == CourseLevel.Recurrent)
                            {
                                // Check if RelatedCourseId is not null for Recurrent courses
                                if (string.IsNullOrEmpty(course.RelatedCourseId))
                                    throw new InvalidOperationException($"Related course ID is missing for recurrent course {course.CourseId}");

                                // Get the Initial certificate for this trainee
                                cert = (await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                                    c.UserId == trainee.TraineeId &&
                                    c.CourseId == course.RelatedCourseId))
                                    .OrderByDescending(c => c.IssueDate)
                                    .FirstOrDefault();
                            }
                            else
                            {
                                // Get certificate for this course
                                cert = (await _unitOfWork.CertificateRepository.GetAllAsync(c =>
                                    c.UserId == trainee.TraineeId &&
                                    c.CourseId == request.CourseId))
                                    .OrderByDescending(c => c.IssueDate)
                                    .FirstOrDefault();
                            }

                            if (cert != null)
                                matchedCertificates.Add(cert);
                        }

                        // Make sure we have certificates, but only if not an Initial course
                        // For Initial courses, we might create decisions before certificates
                        if (!matchedCertificates.Any() && course.CourseLevel != CourseLevel.Initial)
                            throw new InvalidOperationException("No matching certificates found for the course");

                        certificates = matchedCertificates;

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
                            default:
                                templateNamePrefix = "Initial";
                                break;
                        }

                        // Tìm template phù hợp nhất theo tên
                        var decisionTemplates = await _unitOfWork.DecisionTemplateRepository.GetAllAsync(
                            dt => dt.TemplateName.StartsWith(templateNamePrefix) && dt.TemplateStatus == 1);

                        // Lấy template mới nhất (giả sử CreatedAt là thời gian tạo)
                        var latestTemplate = decisionTemplates
                            .OrderByDescending(dt => dt.CreatedAt)
                            .FirstOrDefault();

                        // Nếu không tìm thấy template cụ thể, sử dụng fallback strategy
                        if (latestTemplate == null && templateNamePrefix == "Relearn")
                        {
                            // Fallback: sử dụng template Recurrent cho Relearn nếu không có template riêng
                            latestTemplate = (await _unitOfWork.DecisionTemplateRepository.GetAllAsync(
                                dt => dt.TemplateName.StartsWith("Initial") && dt.TemplateStatus == 1))
                                .OrderByDescending(dt => dt.CreatedAt)
                                .FirstOrDefault();
                        }

                        if (latestTemplate == null)
                            throw new InvalidOperationException($"No active decision template found for {course.CourseLevel} course level");

                        // 3. Lấy template HTML từ blob
                        string templateHtml = "";
                        if (!string.IsNullOrEmpty(latestTemplate.TemplateContent) && latestTemplate.TemplateContent.StartsWith("https"))
                        {
                            using (var httpClient = new HttpClient())
                            {
                                var sasUrl = await _blobService.GetBlobUrlWithSasTokenAsync(latestTemplate.TemplateContent, TimeSpan.FromHours(1), "r");
                                templateHtml = await httpClient.GetStringAsync(sasUrl);
                            }
                        }
                        else if (!string.IsNullOrEmpty(latestTemplate.TemplateContent))
                        {
                            templateHtml = latestTemplate.TemplateContent;
                        }
                        else
                        {
                            throw new InvalidOperationException("Template content is empty");
                        }

                        // 4. Chuẩn bị dữ liệu
                        var decisionCode = GenerateDecisionCode();
                        var issueDate = DateTime.Now;

                        // Generate student rows (dùng certificates nếu có, fallback to trainees)
                        string studentRows = await GenerateStudentRowsAsync(certificates.Any() ? certificates :
                            traineeAssigns.Select(ta => new Certificate { UserId = ta.TraineeId, CourseId = request.CourseId }));

                        var courseSchedules = await _unitOfWork.TrainingScheduleRepository
                            .GetAllAsync(ts => ts.ClassSubject.SubjectSpecialty.SubjectId == request.CourseId);

                        var startDate = courseSchedules.Any() ? courseSchedules.Min(s => s.StartDateTime) : issueDate;
                        var endDate = courseSchedules.Any() ? courseSchedules.Max(s => s.EndDateTime) : issueDate;

                        // 5. Điền dữ liệu vào template
                        string decisionContent = templateHtml
                            .Replace("{{DecisionCode}}", decisionCode)
                            .Replace("{{Day}}", issueDate.Day.ToString())
                            .Replace("{{Month}}", issueDate.Month.ToString())
                            .Replace("{{Year}}", issueDate.Year.ToString())
                            .Replace("{{CourseCode}}", course.CourseId)
                            .Replace("{{CourseTitle}}", !string.IsNullOrEmpty(course.CourseName) ? course.CourseName : $"Khóa {course.CourseId}")
                            .Replace("{{StudentCount}}", certificates.Any() ? certificates.Count().ToString() : traineeAssigns.Count().ToString())
                            .Replace("{{StartDate}}", startDate.ToString("dd/MM/yyyy"))
                            .Replace("{{EndDate}}", endDate.ToString("dd/MM/yyyy"))
                            .Replace("{{StudentRows}}", studentRows);

                        // 6. Lưu Quyết định vào blob
                        string blobName = $"decision_{decisionCode}_{DateTime.Now:yyyyMMddHHmmss}.html";
                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(decisionContent));
                        var blobUrl = await _blobService.UploadFileAsync("decisions", blobName, stream, "text/html");
                        var blobUrlWithoutSas = _blobService.GetBlobUrlWithoutSasToken(blobUrl);

                        // 7. Tạo Decision entity
                        var decision = new Decision
                        {
                            DecisionId = Guid.NewGuid().ToString(),
                            DecisionCode = decisionCode,
                            Title = $"Quyết định cho khóa học {course.CourseName}",
                            Content = blobUrlWithoutSas,
                            IssueDate = issueDate,
                            IssuedByUserId = issuedByUserId,
                            DecisionTemplateId = latestTemplate.DecisionTemplateId,
                            DecisionStatus = DecisionStatus.Draft, // Draft status as enum
                            CertificateId = certificates.FirstOrDefault()?.CertificateId // could be null for Initial if no certs
                        };

                        // 8. Save decision
                        await _unitOfWork.DecisionRepository.AddAsync(decision);
                        await _unitOfWork.SaveChangesAsync();

                        // 9. Update certificates with decision code
                        var courseCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                            c => c.CourseId == request.CourseId && c.Status == CertificateStatus.Active);

                        if (courseCertificates.Any())
                        {
                            using (var httpClient = new HttpClient())
                            {
                                foreach (var cert in courseCertificates)
                                {
                                    if (string.IsNullOrEmpty(cert.CertificateURL))
                                        continue;

                                    try
                                    {
                                        var sasUrl = await _blobService.GetBlobUrlWithSasTokenAsync(cert.CertificateURL, TimeSpan.FromMinutes(5), "r");
                                        string currentHtml = await httpClient.GetStringAsync(sasUrl);
                                        string updatedHtml = currentHtml.Replace("[MÃ QUYẾT ĐỊNH]", decision.DecisionCode);

                                        using var certStream = new MemoryStream(Encoding.UTF8.GetBytes(updatedHtml));
                                        var uri = new Uri(cert.CertificateURL);
                                        var blobNameCert = uri.Segments.Last();
                                        await _blobService.UploadFileAsync("certificates", blobNameCert, certStream, "text/html");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error updating certificate {cert.CertificateId}: {ex.Message}");
                                    }
                                }
                            }
                        }

                        // 10. Notify for signature
                        await NotifyHeadMasterForSignatureAsync(decision.DecisionId, course.CourseName);

                        // 11. Commit the transaction
                        await transaction.CommitAsync(cancellationToken);

                        // 12. Map to response
                        return _mapper.Map<CreateDecisionResponse>(decision);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw new InvalidOperationException($"Error creating decision: {ex.Message}", ex);
                    }
                },
                verifySucceeded: null, // Optional verification logic (not needed here)
                cancellationToken: default);
        }
        #endregion

        #region Get all Draft Decisions
        public async Task<IEnumerable<DecisionModel>> GetAllDraftDecisionsAsync()
        {
            return await GetDecisionsWithSasAsync(async () =>
                await _unitOfWork.DecisionRepository.GetAllAsync(
                    d => d.DecisionStatus == DecisionStatus.Draft));
        }
        #endregion

        #region Get All Sign Decision
        public async Task<IEnumerable<DecisionModel>> GetAllSignDecisionsAsync()
        {
            return await GetDecisionsWithSasAsync(async () =>
                await _unitOfWork.DecisionRepository.GetAllAsync(
                    d => d.DecisionStatus == DecisionStatus.Signed));
        }
        #endregion

        #region Delete Decision
        public async Task<bool> DeleteDecisionAsync(string decisionId)
        {
            if (string.IsNullOrEmpty(decisionId))
                throw new ArgumentException("DecisionId is required", nameof(decisionId));

            var decision = await _unitOfWork.DecisionRepository.GetByIdAsync(decisionId);
            if (decision == null)
                throw new InvalidOperationException($"Decision with ID {decisionId} not found");

            // Check if this is a signed decision
            if (decision.DecisionStatus == DecisionStatus.Signed)
                throw new InvalidOperationException("Cannot delete a signed decision");

            try
            {
                // Delete the decision
                await _unitOfWork.DecisionRepository.DeleteAsync(decisionId);
                await _unitOfWork.SaveChangesAsync();

                // Delete the blob file if it exists
                if (!string.IsNullOrEmpty(decision.Content))
                {
                    await _blobService.DeleteFileAsync(decision.Content);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete decision: {ex.Message}", ex);
            }
        }
        #endregion

        #region Helper Methods
        private string GenerateDecisionCode()
        {
            return $"QD-{DateTime.Now.Year}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
        }

        private async Task<string> GenerateStudentRowsAsync(IEnumerable<Certificate> certificates)
        {
            var studentRows = new StringBuilder();
            int index = 1;

            foreach (var cert in certificates)
            {
                try
                {
                    var trainee = await _userRepository.GetByIdAsync(cert.UserId);
                    if (trainee != null)
                    {
                        string specialtyId = trainee.SpecialtyId ?? "Chưa xác định";
                        var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(specialtyId);
                        var course = await _unitOfWork.CourseRepository.GetByIdAsync(cert.CourseId);

                        studentRows.AppendLine("<tr>");
                        studentRows.AppendLine($"  <td>{index}</td>");
                        studentRows.AppendLine($"  <td>{trainee.FullName}</td>");
                        studentRows.AppendLine($"  <td>{trainee.Username}</td>");
                        studentRows.AppendLine($"  <td>{specialty?.SpecialtyName ?? "Chưa xác định"}</td>");

                        // Thêm cột Ghi chú cho các khóa không phải Initial
                        if (course != null && course.CourseLevel != CourseLevel.Initial)
                            studentRows.AppendLine("  <td></td>");

                        studentRows.AppendLine("</tr>");
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other students
                    // Consider adding proper logging here
                    Console.WriteLine($"Error processing student {cert.UserId}: {ex.Message}");
                }
            }

            return studentRows.ToString();
        }

        private async Task NotifyHeadMasterForSignatureAsync(string decisionId, string courseName)
        {
            var headMasters = await _userRepository.GetUsersByRoleAsync("HeadMaster");

            if (headMasters == null || !headMasters.Any())
                return; // No headmasters to notify

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

        private async Task<IEnumerable<DecisionModel>> GetDecisionsWithSasAsync(
            Func<Task<IEnumerable<Decision>>> getDecisionsFunc)
        {
            if (getDecisionsFunc == null)
                throw new ArgumentNullException(nameof(getDecisionsFunc));

            var decisions = await getDecisionsFunc();

            if (decisions == null || !decisions.Any())
                return new List<DecisionModel>();

            var decisionsWithSas = new List<DecisionModel>();
            foreach (var decision in decisions)
            {
                try
                {
                    if (string.IsNullOrEmpty(decision.Content))
                    {
                        var decisionModel = _mapper.Map<DecisionModel>(decision);
                        decisionsWithSas.Add(decisionModel);
                        continue;
                    }

                    var contentWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(
                        decision.Content, TimeSpan.FromHours(1), "r");

                    var decisionModelWithSas = _mapper.Map<DecisionModel>(decision);
                    decisionModelWithSas.ContentWithSas = contentWithSas;
                    decisionsWithSas.Add(decisionModelWithSas);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other decisions
                    // Consider adding proper logging here
                    Console.WriteLine($"Error processing decision {decision.DecisionId}: {ex.Message}");

                    // Still add the decision to the list even without SAS URL
                    var decisionModelWithoutSas = _mapper.Map<DecisionModel>(decision);
                    decisionsWithSas.Add(decisionModelWithoutSas);
                }
            }

            return decisionsWithSas;
        }
        #endregion
    }
}