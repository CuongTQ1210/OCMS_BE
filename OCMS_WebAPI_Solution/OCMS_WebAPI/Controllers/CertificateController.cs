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

        #region Create Certificate
        [HttpPost("create")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateCertificate([FromBody] CreateCertificateDTO dto)
        {
            try
            {
                // Lấy UserId từ Claims
                var issuedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(issuedByUserId))
                {
                    return Unauthorized("User identity not found");
                }

                var result = await _certificateService.CreateCertificateAsync(dto, issuedByUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Xử lý exception
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region Get Certificates by TraineeId
        [HttpGet("trainee/{traineeId}")]
        [CustomAuthorize("Admin", "Training staff", "Trainee", "AOC Manager")]
        public async Task<IActionResult> GetCertificatesByTraineeId(string traineeId)
        {
            try
            {
                var result = await _certificateService.GetCertificatesByTraineeIdAsync(traineeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Xử lý exception
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region Delete Certificate
        [HttpDelete("{certificateId}")]
        public async Task<IActionResult> DeleteCertificate(string certificateId)
        {
            try
            {
                var result = await _certificateService.DeleteCertificateAsync(certificateId);
                if (result)
                {
                    return Ok("Certificate deleted successfully");
                }
                else
                {
                    return NotFound("Certificate not found");
                }
            }
            catch (Exception ex)
            {
                // Xử lý exception
                return StatusCode(500, ex.Message);
            }
        }
        #endregion
    }
}
