using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_Services.IService;
using OCMS_Services.Service;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

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

        #region Export Reports
        [HttpGet("export-expired-certificates")]
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        public async Task<IActionResult> ExportExpiredCertificates()
        {
            var data = await _reportService.GetExpiredCertificatesAsync();

            if (!data.Any())
                return NotFound("No expired certificates found.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var fileName = $"ExpiredCertificates_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            // Generate the report and save it
            var (fileBytes, report) = await _reportService.GenerateExcelReport(data, filePath, userId);

            // Clean up the temporary file
            System.IO.File.Delete(filePath);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        #endregion

        #region Export Trainee Info
        [HttpGet("export-trainee-info/{traineeId}")]
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        public async Task<IActionResult> ExportTraineeInfo(string traineeId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var (fileBytes, report) = await _reportService.ExportTraineeInfoToExcelAsync(traineeId, userId);
                var fileName = $"TraineeInfo_{traineeId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Export Course Result
        [HttpGet("export-course-result")]
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        public async Task<IActionResult> ExportCourseResult()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var (fileBytes, report) = await _reportService.ExportCourseResultReportToExcelAsync(userId);
            var fileName = $"CourseResultReport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        #endregion
        // 4️⃣ View Saved Reports
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        [HttpGet("saved-reports")]
        public async Task<IActionResult> GetSavedReports()
        {
            var reports = await _reportService.GetSavedReportsAsync();
            return Ok(reports);
        }
    }
}
