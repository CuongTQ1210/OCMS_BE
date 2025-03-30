using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateTemplateController : ControllerBase
    {
        private readonly ICertificateTemplateService _certificateTemplateService;

        public CertificateTemplateController(ICertificateTemplateService certificateTemplateService)
        {
            _certificateTemplateService = certificateTemplateService;
        }

        #region Import Certificate Template
        [HttpPost]
        [CustomAuthorize("Admin")]
        public async Task<ActionResult<CreateCertificateTemplateResponse>> CreateCertificateTemplate([FromForm] CreateCertificateTemplateDTO request)
        {
            if (request.HtmlTemplate == null || request.HtmlTemplate.Length == 0)
            {
                return BadRequest("HTML template file is required.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                using (var stream = request.HtmlTemplate.OpenReadStream())
                {
                    var createdTemplate = await _certificateTemplateService.CreateCertificateTemplateAsync(request, currentUserId);
                    return CreatedAtAction(nameof(GetCertificateTemplate),
                        new { templateId = createdTemplate.CertificateTemplateId },
                        createdTemplate);
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Get Certificate Template
        [HttpGet("{templateId}")]
        [CustomAuthorize("Admin")]
        public async Task<ActionResult<GetCertificateTemplateResponse>> GetCertificateTemplate(string templateId)
        {
            var template = await _certificateTemplateService.GetCertificateTemplateByIdAsync(templateId);

            if (template == null)
            {
                return NotFound($"Certificate template with ID {templateId} not found.");
            }

            return Ok(template);
        }
        #endregion

        #region Get All Certificate Templates
        [HttpGet]
        [CustomAuthorize("Admin")]
        public async Task<ActionResult<GetAllCertificateTemplatesResponse>> GetAllCertificateTemplates()
        {
            var templates = await _certificateTemplateService.GetAllCertificateTemplatesAsync();
            return Ok(templates);
        }
        #endregion

        #region Delete Certificate Template
        [HttpDelete("{templateId}")]
        public async Task<IActionResult> DeleteCertificateTemplate(string templateId)
        {
            // Lấy ID của người dùng hiện tại từ claims
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Gọi service để xóa template
            bool result = await _certificateTemplateService.DeleteCertificateTemplateAsync(templateId, currentUserId);

            if (result)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Certificate template deleted successfully"
                });
            }
            else
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Certificate template not found or could not be deleted"
                });
            }
        }
        #endregion

        #region Update Certificate Template
        [HttpPut("{templateId}")]
        public async Task<IActionResult> UpdateCertificateTemplate(string templateId, [FromForm] UpdateCertificateTemplateDTO request)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _certificateTemplateService.UpdateCertificateTemplateAsync(templateId, request, currentUserId);

                if (response == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Certificate template not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Certificate template updated successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while updating the certificate template",
                    Error = ex.Message
                });
            }
        }
        #endregion
    }
}
