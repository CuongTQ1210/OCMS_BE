using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class ReportService :  IReportService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICertificateService _certificateService;
        private readonly IBlobService _storageService; // New dependency for Blob Storage
        private const string ReportContainerName = "reports"; // Blob container name for reports

        public ReportService(UnitOfWork unitOfWork, IMapper mapper, ICertificateService certificateService, IBlobService storageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _certificateService = certificateService;
            _storageService = storageService;
        }

        // Helper method to save a report to the database and upload the file to Blob Storage
        private async Task<Report> SaveReportAsync(string reportName, ReportType reportType, byte[] fileBytes, string fileName, string generateByUserId, DateTime startDate, DateTime endDate, string content)
        {
            // Upload the file to Azure Blob Storage
            using var stream = new MemoryStream(fileBytes);
            var fileUrl = await _storageService.UploadFileAsync(
                containerName: ReportContainerName,
                blobName: fileName,
                fileStream: stream,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );

            // Create the Report entity
            var report = new Report
            {
                ReportId = $"R-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                ReportName = reportName,
                ReportType = reportType,
                GenerateByUserId = generateByUserId,
                GenerateDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                Content = content,
                Format = "Excel",
                FileUrl = fileUrl // Store the Blob Storage URL
            };

            // Save to database
            await _unitOfWork.ReportRepository.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            return report;
        }
        public async Task<List<ReportModel>> GetSavedReportsAsync()
        {
            var reports = await _unitOfWork.ReportRepository
                .GetQueryable()
                .Include(r => r.GenerateByUser)
                .ToListAsync();

            return reports.Select(r => new ReportModel
            {
                ReportId = r.ReportId,
                ReportName = r.ReportName,
                ReportType = r.ReportType.ToString(),
                GenerateByUserId = r.GenerateByUserId,
                GenerateByUserName = r.GenerateByUser?.FullName ?? "Unknown",
                GenerateDate = r.GenerateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                StartDate = r.StartDate.ToString("yyyy-MM-dd"),
                EndDate = r.EndDate.ToString("yyyy-MM-dd"),
                Content = r.Content,
                Format = r.Format,
                FileUrl = r.FileUrl
            }).ToList();
        }
        public async Task<(byte[] fileBytes, Report report)> GenerateExcelReport(List<ExpiredCertificateReportDto> data, string filePath, string generateByUserId)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Expired Certificates");

            // Headers
            sheet.Cells[1, 1].Value = "User ID";
            sheet.Cells[1, 2].Value = "Course ID";
            sheet.Cells[1, 3].Value = "Status";
            sheet.Cells[1, 4].Value = "Issue Date";
            sheet.Cells[1, 5].Value = "Expiration Date";
            sheet.Cells[1, 6].Value = "Certificate Link"; // 🆕

            // Data
            int row = 2;
            foreach (var item in data)
            {
                sheet.Cells[row, 1].Value = item.UserId;
                sheet.Cells[row, 2].Value = item.CourseId;
                sheet.Cells[row, 3].Value = item.Status.ToString();
                sheet.Cells[row, 4].Value = item.IssueDate.ToShortDateString();
                sheet.Cells[row, 5].Value = item.ExpirationDate?.ToShortDateString();
                sheet.Cells[row, 6].Hyperlink = new Uri(item.CertificateUrlWithSas ?? "#");
                sheet.Cells[row, 6].Value = "View Certificate";
                sheet.Cells[row, 6].Style.Font.UnderLine = true;
                sheet.Cells[row, 6].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                row++;
            }

            sheet.Cells.AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));

            // Read the file bytes for upload and return
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            // Save the report to the database and upload to Blob Storage
            var fileName = Path.GetFileName(filePath);
            var startDate = data.Min(c => c.IssueDate);
            var endDate = data.Max(c => c.ExpirationDate ?? DateTime.UtcNow);
            var content = $"Expired Certificates Report: {data.Count} certificates found.";
            var report = await SaveReportAsync(
                reportName: "Expired Certificates Report",
                reportType: ReportType.ExpiredCertificate,
                fileBytes: fileBytes,
                fileName: fileName,
                generateByUserId: generateByUserId,
                startDate: startDate,
                endDate: endDate,
                content: content
            );

            return (fileBytes, report);
        }
        public async Task<List<ExpiredCertificateReportDto>> GetExpiredCertificatesAsync()
        {
            var now = DateTime.Now;
            var fourMonthsLater = now.AddMonths(4);

            var certificates = await _certificateService.GetActiveCertificatesWithSasUrlAsync();

            var expiredCerts = certificates
                .Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value <= fourMonthsLater)
                .Select(c => new ExpiredCertificateReportDto
                {
                    UserId = c.UserId,
                    CourseId = c.CourseId,
                    Status = c.Status,
                    IssueDate = c.IssueDate,
                    ExpirationDate = c.ExpirationDate,
                    CertificateUrlWithSas = c.CertificateURLwithSas // 🆕 Include link
                })
                .ToList();

            return expiredCerts;
        }
        public async Task<(byte[] fileBytes, Report report)> ExportTraineeInfoToExcelAsync(string traineeId, string generateByUserId)
        {
            var reportData = await GenerateTraineeInfoReportByTraineeIdAsync(traineeId);

            if (reportData == null || !reportData.Any())
                throw new Exception("No data found for the specified trainee.");

            var traineeInfo = reportData.First();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Trainee Info");

            // Merge Trainee info (1st row)
            string traineeInfoText = $"Trainee: {traineeInfo.TraineeName} | ID: {traineeInfo.TraineeId} | Email: {traineeInfo.Email}";
            worksheet.Cells[1, 1].Value = traineeInfoText;
            worksheet.Cells[1, 1, 1, 6].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 12;

            // Header row (2nd row)
            worksheet.Cells[2, 1].Value = "Course ID";
            worksheet.Cells[2, 2].Value = "Course Name";
            worksheet.Cells[2, 3].Value = "Assign Date";
            worksheet.Cells[2, 4].Value = "Subject ID";
            worksheet.Cells[2, 5].Value = "Total Grade";
            worksheet.Cells[2, 6].Value = "Status";

            // Content rows (start from 3rd row)
            int row = 3;
            foreach (var item in reportData)
            {
                worksheet.Cells[row, 1].Value = item.CourseId;
                worksheet.Cells[row, 2].Value = item.CourseName;
                worksheet.Cells[row, 3].Value = item.AssignDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = item.SubjectId;
                worksheet.Cells[row, 5].Value = item.TotalGrade;
                worksheet.Cells[row, 6].Value = item.Status;
                row++;
            }

            // Optional: auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileBytes = await package.GetAsByteArrayAsync();

            // Save the report to the database and upload to Blob Storage
            var fileName = $"TraineeInfo_{traineeId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var startDate = reportData.Min(r => r.AssignDate);
            var endDate = DateTime.UtcNow;
            var content = $"Trainee Info Report for Trainee ID: {traineeId}, {reportData.Count} records.";
            var report = await SaveReportAsync(
                reportName: $"Trainee Info Report - {traineeId}",
                reportType: ReportType.TraineeResult,
                fileBytes: fileBytes,
                fileName: fileName,
                generateByUserId: generateByUserId,
                startDate: startDate,
                endDate: endDate,
                content: content
            );

            return (fileBytes, report);
        }
        public async Task<List<TraineeInfoReportDto>> GenerateTraineeInfoReportByTraineeIdAsync(string traineeId)
        {
            var traineeAssigns = await _unitOfWork.TraineeAssignRepository
                .GetQueryable()
                .Where(x => x.TraineeId == traineeId)
                .Include(x => x.Trainee)
                .Include(x => x.CourseSubjectSpecialty)
                    .ThenInclude(css => css.Course)
                .Include(x => x.CourseSubjectSpecialty)
                    .ThenInclude(css => css.Subject)
                .ToListAsync();

            var grades = await _unitOfWork.GradeRepository
                .GetQueryable()
                .Include(g => g.TraineeAssign)
                .Where(g => g.TraineeAssign.TraineeId == traineeId)
                .ToListAsync();

            var report = (from assign in traineeAssigns
                          join grade in grades on assign.TraineeAssignId equals grade.TraineeAssignID into gj
                          from subGrade in gj.DefaultIfEmpty()
                          select new TraineeInfoReportDto
                          {
                              TraineeId = assign.TraineeId,
                              TraineeName = assign.Trainee?.FullName,
                              Email = assign.Trainee?.Email,
                              CourseId = assign.CourseSubjectSpecialty.CourseId,
                              CourseName = assign.CourseSubjectSpecialty.Course?.CourseName ?? "Unknown",
                              AssignDate = assign.AssignDate,
                              SubjectId = assign.CourseSubjectSpecialty.SubjectId,
                              TotalGrade = subGrade?.TotalScore,
                              Status = subGrade == null
                                  ? "N/A"
                                  : (subGrade.TotalScore >= (assign.CourseSubjectSpecialty.Subject?.PassingScore ?? 5) ? "Pass" : "Fail")
                          }).ToList();

            return report;
        }

        public async Task<List<CourseResultReportDto>> GenerateAllCourseResultReportAsync()
        {
            var traineeAssigns = await _unitOfWork.TraineeAssignRepository
                .GetQueryable()
                .Include(x => x.CourseSubjectSpecialty)
                    .ThenInclude(css => css.Course)
                .Include(x => x.CourseSubjectSpecialty)
                    .ThenInclude(css => css.Subject)
                .ToListAsync();

            var traineeAssignIds = traineeAssigns.Select(x => x.TraineeAssignId).ToList();

            var grades = await _unitOfWork.GradeRepository
                .GetQueryable()
                .Include(g => g.TraineeAssign)
                    .ThenInclude(ta => ta.CourseSubjectSpecialty)
                        .ThenInclude(css => css.Course)
                .Include(g => g.TraineeAssign)
                    .ThenInclude(ta => ta.CourseSubjectSpecialty)
                        .ThenInclude(css => css.Subject)
                .Where(g => traineeAssignIds.Contains(g.TraineeAssignID))
                .ToListAsync();

            var report = (from assign in traineeAssigns
                          join grade in grades on assign.TraineeAssignId equals grade.TraineeAssignID
                          where grade.TraineeAssign.CourseSubjectSpecialty != null
                          select new
                          {
                              CourseId = assign.CourseSubjectSpecialty.CourseId,
                              TotalScore = grade.TotalScore,
                              PassingScore = assign.CourseSubjectSpecialty.Subject.PassingScore
                          })
                          .GroupBy(x => x.CourseId)
                          .Select(g => new CourseResultReportDto
                          {
                              CourseId = g.Key,
                              SubjectId = null, // Not grouping by SubjectId anymore
                              TotalTrainees = g.Count(),
                              PassCount = g.Count(x => x.TotalScore >= x.PassingScore),
                              FailCount = g.Count(x => x.TotalScore < x.PassingScore),
                              AverageScore = Math.Round(g.Average(x => x.TotalScore), 2)
                          })
                          .ToList();

            return report;
        }
        public async Task<(byte[] fileBytes, Report report)> ExportCourseResultReportToExcelAsync(string generateByUserId)
        {
            var reportData = await GenerateAllCourseResultReportAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Course Result Report");

            // Header
            worksheet.Cells[1, 1].Value = "Course ID";
            worksheet.Cells[1, 2].Value = "Total Trainees";
            worksheet.Cells[1, 3].Value = "Pass Count";
            worksheet.Cells[1, 4].Value = "Fail Count";
            worksheet.Cells[1, 5].Value = "Average Score";

            // Style the header
            using (var range = worksheet.Cells[1, 1, 1, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Data rows
            int row = 2;
            foreach (var item in reportData)
            {
                worksheet.Cells[row, 1].Value = item.CourseId;
                worksheet.Cells[row, 2].Value = item.TotalTrainees;
                worksheet.Cells[row, 3].Value = item.PassCount;
                worksheet.Cells[row, 4].Value = item.FailCount;
                worksheet.Cells[row, 5].Value = item.AverageScore;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var fileBytes = await package.GetAsByteArrayAsync();

            // Save the report to the database and upload to Blob Storage
            var fileName = $"CourseResultReport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var startDate = DateTime.UtcNow.AddMonths(-12); // Example: Last 12 months
            var endDate = DateTime.UtcNow;
            var content = $"Course Result Report: {reportData.Count} courses reported.";
            var report = await SaveReportAsync(
                reportName: "Course Result Report",
                reportType: ReportType.CourseResult,
                fileBytes: fileBytes,
                fileName: fileName,
                generateByUserId: generateByUserId,
                startDate: startDate,
                endDate: endDate,
                content: content
            );

            return (fileBytes, report);
        }

    }
}
