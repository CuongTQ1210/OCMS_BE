using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_Services.IService;
using OCMS_Services.Service;
using OCMS_WebAPI.AuthorizeSettings;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // 1️⃣ Expired Certificate Report
        [CustomAuthorize("Admin","HR", "Reviewer")]
        [HttpGet("export-expired-certificates")]
        public async Task<IActionResult> ExportExpiredCertificates()
        {
            var data = await _reportService.GetExpiredCertificatesAsync();

            if (!data.Any())
                return NotFound("No expired certificates found.");

            var fileName = $"ExpiredCertificates_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            await _reportService.GenerateExcelReport(data, filePath);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            System.IO.File.Delete(filePath); // Clean up temp file

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // 2️⃣ Trainee Info Report (by trainee ID)
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        [HttpGet("export-trainee-info/{traineeId}")]
        public async Task<IActionResult> ExportTraineeInfo(string traineeId)
        {
            try
            {
                var excelFile = await _reportService.ExportTraineeInfoToExcelAsync(traineeId);
                var fileName = $"TraineeInfo_{traineeId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(
                    excelFile,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 3️⃣ Course Result Report (All Courses)
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        [HttpGet("export-course-result")]
        public async Task<IActionResult> ExportCourseResult()
        {
            var excelFile = await _reportService.ExportCourseResultReportToExcelAsync();
            var fileName = $"CourseResultReport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            return File(
                excelFile,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}
