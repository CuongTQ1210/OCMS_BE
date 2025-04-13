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

        //#region AutoGenerateCertificatesForPassedTrainees
        //[HttpPost("AutoGenerateCertificatesForPassedTrainees")]
        //[ValidateAntiForgeryToken]
        //[CustomAuthorize("Admin", "Training staff")]
        //public async Task<IActionResult> AutoGenerateCertificatesForPassedTrainees([FromBody] string courseId)
        //{
        //    if (string.IsNullOrEmpty(courseId))
        //    {
        //        return BadRequest("Course ID cannot be null or empty.");
        //    }
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return Unauthorized("User ID not found in claims.");
        //    }
        //    var result = await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(courseId, userId);
        //    if (result == null || result.Count == 0)
        //    {
        //        return NotFound("No certificates generated.");
        //    }
        //    return Ok(result);
        //}
        //#endregion
    }
}
