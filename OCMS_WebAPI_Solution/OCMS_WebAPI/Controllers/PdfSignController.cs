using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_Services.IService;
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

        //[HttpPost("sign")]
        //public async Task<IActionResult> SignPdf(IFormFile file)
        //{
        //    // Validate the file
        //    if (file == null || file.Length == 0)
        //        return BadRequest("A file is required.");

        //    // Check file extension (optional, to ensure it's HTML)
        //    if (!Path.GetExtension(file.FileName).ToLower().Equals(".html"))
        //        return BadRequest("Only HTML files are supported.");

        //    // Read the file content as a string
        //    string htmlContent;
        //    using (var stream = new StreamReader(file.OpenReadStream()))
        //    {
        //        htmlContent = await stream.ReadToEndAsync();
        //    }

        //    // Validate HTML content
        //    if (string.IsNullOrWhiteSpace(htmlContent))
        //        return BadRequest("The uploaded file is empty or invalid.");

        //    // Call the service to process the HTML content
        //    var result = await _pdfSignerService.SignPdfAsync(htmlContent);

        //    return Ok(result);
        //}

        //[HttpPost("sign-from-url")]
        //public async Task<IActionResult> SignPdfFromUrl([FromBody] string url)
        //{
        //    // Validate the input URL
        //    if (string.IsNullOrWhiteSpace(url))
        //        return BadRequest("A valid URL is required.");

        //    try
        //    {
        //        // Sign the PDF using the HTML content
        //        var result = await _pdfSignerService.SignPdfAsync(url);

        //        // Deserialize and extract file_data
        //        var jsonDoc = JsonDocument.Parse(result);
        //        var fileData = jsonDoc.RootElement
        //            .GetProperty("result")
        //            .GetProperty("file_data")
        //            .GetString();

        //        // Convert base64 back to PDF bytes
        //        byte[] signedPdfBytes = Convert.FromBase64String(fileData);

        //        // Return the signed PDF file as download
        //        return File(signedPdfBytes, "application/pdf", "signed-document.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while processing the URL: {ex.Message}");
        //    }

        //}
        [HttpPost("{certificateId}")]
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

    }
}
