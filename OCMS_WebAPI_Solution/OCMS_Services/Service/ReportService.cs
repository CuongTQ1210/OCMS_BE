using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.RequestModel;
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
        public ReportService(UnitOfWork unitOfWork, IMapper mapper, ICertificateService certificateService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _certificateService = certificateService;
        }
        public async Task GenerateExcelReport(List<ExpiredCertificateReportDto> data, string filePath)
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
        public async Task<byte[]> ExportTraineeInfoToExcelAsync(string traineeId)
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

            return await package.GetAsByteArrayAsync();
        }
        public async Task<List<TraineeInfoReportDto>> GenerateTraineeInfoReportByTraineeIdAsync(string traineeId)
        {
            var traineeAssigns = await _unitOfWork.TraineeAssignRepository
                .GetQueryable()
                .Where(x => x.TraineeId == traineeId)
                .Include(x => x.Trainee)
                .Include(x => x.Course)
                .ToListAsync();

            var grades = await _unitOfWork.GradeRepository
                        .GetQueryable()
                        .Include(g => g.Subject) // Include subject to access PassingScore
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
                              CourseId = assign.CourseId,
                              CourseName = assign.Course?.CourseName,
                              AssignDate = assign.AssignDate,
                              SubjectId = subGrade?.SubjectId,
                              TotalGrade = subGrade?.TotalScore,
                              Status = subGrade == null
        ? "N/A"
        : (subGrade.TotalScore >= (subGrade.Subject?.PassingScore ?? 5) ? "Pass" : "Fail")
                          }).ToList();

            return report;
        }

        public async Task<List<CourseResultReportDto>> GenerateAllCourseResultReportAsync()
        {
            // Step 1: Get all TraineeAssigns with Course & Grade info
            var traineeAssigns = await _unitOfWork.TraineeAssignRepository
                .GetQueryable()
                .Include(x => x.Course)
                .ToListAsync();

            var traineeAssignIds = traineeAssigns.Select(x => x.TraineeAssignId).ToList();

            var grades = await _unitOfWork.GradeRepository
                .GetQueryable()
                .Include(g => g.Subject)
                .Where(g => traineeAssignIds.Contains(g.TraineeAssignID))
                .ToListAsync();

            // Step 2: Join Grades with their CourseId via TraineeAssign
            var report = (from assign in traineeAssigns
                          join grade in grades on assign.TraineeAssignId equals grade.TraineeAssignID
                          where grade.Subject != null
                          select new
                          {
                              assign.CourseId,
                              grade.SubjectId,
                              grade.TotalScore,
                              grade.Subject.PassingScore
                          })
                          .GroupBy(x => new { x.CourseId, x.SubjectId })
                          .Select(g => new CourseResultReportDto
                          {
                              CourseId = g.Key.CourseId,
                              SubjectId = g.Key.SubjectId,
                              TotalTrainees = g.Count(),
                              PassCount = g.Count(x => x.TotalScore >= x.PassingScore),
                              FailCount = g.Count(x => x.TotalScore < x.PassingScore),
                              AverageScore = Math.Round(g.Average(x => x.TotalScore), 2)
                          })
                          .ToList();

            return report;
        }
        public async Task<byte[]> ExportCourseResultReportToExcelAsync()
        {
            var reportData = await GenerateAllCourseResultReportAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Course Result Report");

            // Header
            worksheet.Cells[1, 1].Value = "Course ID";
            worksheet.Cells[1, 2].Value = "Subject ID";
            worksheet.Cells[1, 3].Value = "Total Trainees";
            worksheet.Cells[1, 4].Value = "Pass Count";
            worksheet.Cells[1, 5].Value = "Fail Count";
            worksheet.Cells[1, 6].Value = "Average Score";

            // Style the header
            using (var range = worksheet.Cells[1, 1, 1, 6])
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
                worksheet.Cells[row, 2].Value = item.SubjectId;
                worksheet.Cells[row, 3].Value = item.TotalTrainees;
                worksheet.Cells[row, 4].Value = item.PassCount;
                worksheet.Cells[row, 5].Value = item.FailCount;
                worksheet.Cells[row, 6].Value = item.AverageScore;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }

    }
}
