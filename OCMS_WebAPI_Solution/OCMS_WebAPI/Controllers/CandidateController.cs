using Microsoft.AspNetCore.Mvc;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidateController : Controller
    {
        private readonly ICandidateService _candidateService;

        public CandidateController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        #region Import Candidates
        [HttpPost("import")]
        [CustomAuthorize("Admin", "HR")]
        public async Task<IActionResult> ImportUsers(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file == null || file.Length == 0)
                return BadRequest("No file was uploaded");

            using (var stream = file.OpenReadStream())
            {
                var result = await _candidateService.ImportCandidatesFromExcelAsync(stream, userId);
                return Ok(result);
            }
        }
        #endregion

        #region Get All Candidates
        [HttpGet]
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        public async Task<IActionResult> GetAllCandidates()
        {
            try
            {
                var candidates = await _candidateService.GetAllCandidates();
                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Candidate By Id
        [HttpGet("{id}")]
        [CustomAuthorize("Admin", "HR", "Reviewer")]
        public async Task<IActionResult> GetCandidateById(string id)
        {
            try
            {
                var candidate = await _candidateService.GetCandidateByIdAsync(id);
                return Ok(candidate);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
