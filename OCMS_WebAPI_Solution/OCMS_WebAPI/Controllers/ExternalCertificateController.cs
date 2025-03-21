using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.ViewModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalCertificateController : ControllerBase
    {
        private readonly IExternalCertificateService _externalCertificateService;
        private readonly IBlobService _blobService;

        public ExternalCertificateController(IExternalCertificateService externalCertificateService, IBlobService blobService)
        {
            _externalCertificateService = externalCertificateService;
            _blobService = blobService;
        }

        #region Get External Certificates by Candidate Id
        [HttpGet("candidate/{candidateId}")]
        [CustomAuthorize("Admin", "HR")]
        public async Task<IActionResult> GetByCandidateId(string candidateId)
        {
            var certificates = await _externalCertificateService.GetExternalCertificatesByCandidateIdAsync(candidateId);
            return Ok(certificates);
        }
        #endregion

        #region Add External Certificate
        [HttpPost("{candidateId}")]
        [CustomAuthorize("Admin", "HR")]
        public async Task<IActionResult> AddExternalCertificate(string candidateId, [FromForm] ExternalCertificateModel certificateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var certificate = await _externalCertificateService.AddExternalCertificateAsync(candidateId, certificateDto, certificateDto.CertificateImage, _blobService, userId);
                return Ok(certificate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Update External Certificate
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "HR")]
        public async Task<IActionResult> Update(int id, [FromForm] ExternalCertificateModel certificateDto)
        {
            try
            {
                var updatedCertificate = await _externalCertificateService.UpdateExternalCertificateAsync(id, certificateDto, _blobService);

                if (updatedCertificate == null)
                {
                    return NotFound();
                }

                return Ok(updatedCertificate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        #endregion

        #region Delete External Certificate
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "HR")]
        public async Task<IActionResult> DeleteExternalCertificate(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("ID không hợp lệ");
                }

                bool result = await _externalCertificateService.DeleteExternalCertificateAsync(id, _blobService);

                if (result)
                {
                    return Ok(new { Success = true, Message = "Xóa chứng chỉ thành công" });
                }
                else
                {
                    return BadRequest("Không thể xóa chứng chỉ");
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
        #endregion
    }
}

