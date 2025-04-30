using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly IBlobService _blobService;

        public CertificateController(ICertificateService certificateService, IBlobService blobService)
        {
            _certificateService = certificateService;
            _blobService = blobService;
        }
        

        #region Get Pending Certificates
        [HttpGet("pending")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetPendingCertificates()
        {
            try
            {
                var certificates = await _certificateService.GetPendingCertificatesWithSasUrlAsync();
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Certificate By Id
        [HttpGet("{certificateId}")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetCertificateById(string certificateId)
        {
            try
            {
                var certificate = await _certificateService.GetCertificateByIdAsync(certificateId);
                if (certificate == null)
                    return NotFound(new { message = "Certificate not found." });
                return Ok(certificate);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Active Certificates
        [HttpGet("active")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetActiveCertificates()
        {
            try
            {
                var certificates = await _certificateService.GetActiveCertificatesWithSasUrlAsync();
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Certificates By User Id
        [HttpGet("certificate/{userId}")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster", "AOC Manager")]
        public async Task<IActionResult> GetCertificatesByUserId(string userId)
        {
            try
            {
                var certificates = await _certificateService.GetCertificatesByUserIdWithSasUrlAsync(userId);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Trainee View Certificate
        [HttpGet("trainee/view")]
        [CustomAuthorize("Trainee")]
        public async Task<IActionResult> GetCertificatesByTraineeId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var certificates = await _certificateService.GetCertificatesByUserIdWithSasUrlAsync(userId);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Manual Certificate Generation
        [HttpPost("manual-create")]
        [CustomAuthorize("Training staff", "Admin")]
        public async Task<IActionResult> CreateCertificateManually([FromBody]CreateCertificateDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.TraineeId) || string.IsNullOrEmpty(request.CourseId))
                return BadRequest("Trainee ID and Course ID are required");

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var certificate = await _certificateService.CreateCertificateManuallyAsync(
                    request.TraineeId,
                    request.CourseId,
                    userId);

                return Ok(new
                {
                    Message = "Certificate created successfully",
                    Certificate = certificate
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        #endregion

        #region revoke certificate
        [HttpPost("revoke/{certificateId}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> RevokeCertificate(string certificateId, [FromBody] RevokeCertificateDTO dto)
        {
            var result = await _certificateService.RevokeCertificateAsync(certificateId, dto);
            if (!result.success)
                return BadRequest(result.message);

            return Ok(result.message);
        }
        #endregion

        #region get revoked
        [HttpGet("revoked")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetAllRevokedCertificates()
        {
            try
            {
                var certificates = await _certificateService.GetRevokedCertificatesWithSasUrlAsync();
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Certificate Renewal History
        [HttpGet("renewal-history/{certificateId}")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster", "Trainee", "AOC Manager")]
        public async Task<IActionResult> GetCertificateRenewalHistory(string certificateId)
        {
            try
            {
                var result = await _certificateService.GetCertificateRenewalHistoryAsync(certificateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get User Certificate Renewal History
        /// <summary>
        /// Lấy lịch sử gia hạn tất cả chứng chỉ của một người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Danh sách lịch sử gia hạn chứng chỉ của người dùng</returns>
        [HttpGet("user-renewal-history/{userId}")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster", "AOC Manager")]
        public async Task<IActionResult> GetUserCertificateRenewalHistory(string userId)
        {
            try
            {
                var result = await _certificateService.GetUserCertificateRenewalHistoryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Current User's Certificate Renewal History
        /// <summary>
        /// Lấy lịch sử gia hạn chứng chỉ của người dùng hiện tại
        /// </summary>
        /// <returns>Danh sách lịch sử gia hạn chứng chỉ của người dùng hiện tại</returns>
        [HttpGet("my-renewal-history")]
        [CustomAuthorize("Trainee")]
        public async Task<IActionResult> GetCurrentUserCertificateRenewalHistory()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _certificateService.GetUserCertificateRenewalHistoryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
