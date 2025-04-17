using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_Services.IService;
using OCMS_Services.Service;
using OCMS_WebAPI.AuthorizeSettings;
using System.Text.Json;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfSignController : ControllerBase
    {
        private readonly IPdfSignerService _pdfSignerService;

        public PdfSignController(IPdfSignerService pdfSignerService)
        {
            _pdfSignerService = pdfSignerService;
        }
        
        [HttpPost("{certificateId}")]
        [CustomAuthorize("HeadMaster")]
        public async Task<IActionResult> SignPdfFromCertificateId(string certificateId)
        {
            if (string.IsNullOrWhiteSpace(certificateId))
                return BadRequest("A valid certificate ID is required.");

            try
            {
                byte[] signedPdfBytes = await _pdfSignerService.SignPdfAsync(certificateId);
                return File(signedPdfBytes, "application/pdf", "signed-document.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing the PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an email with a SAS URL for the signed certificate.
        /// </summary>
        /// <param name="request">The user email and certificate ID.</param>
        /// <returns>Confirmation of email sent or error details.</returns>
        /// <response code="200">Email sent successfully.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="500">Server error occurred.</response>
        [HttpPost("send/{certificateId}")]
        [CustomAuthorize("Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendCertificateEmail(string certificateId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                
                await _pdfSignerService.SendCertificateByEmailAsync(certificateId);
                return Ok(new { Message = $"Email sent successfully!!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Log the error for debugging (replace with proper logging in production)
                Console.WriteLine($"Error sending certificate email . InnerException: {ex.InnerException?.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to send certificate email.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

        [HttpPost("{decisionId}/sign")]
        [CustomAuthorize("HeadMaster")]
        public async Task<IActionResult> SignDecisionAsync(string decisionId)
        {
            try
            {
                // Validate decisionId
                if (string.IsNullOrWhiteSpace(decisionId))
                {
                    return BadRequest(new { Message = "Decision ID cannot be null or empty." });
                }

                // Call the PdfSignerService to sign the decision
                var signedPdfBytes = await _pdfSignerService.SignDecisionAsync(decisionId);

                // Return the signed PDF as a file
                return File(signedPdfBytes, "application/pdf", $"signed_decision_{decisionId}.pdf");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception (logging should be configured in your application)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while signing the decision.", Details = ex.Message });
            }
        }
    }
}
