using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("trainee/{userId}")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetCertificatesByUserId()
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

        #region Trainee View Certificate
        [HttpGet("trainee/view/{userId}")]
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
    }
}
