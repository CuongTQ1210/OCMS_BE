using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly IBlobService _blobService;

        public CandidateController(ICandidateService candidateService, IBlobService blobService)
        {
            _candidateService = candidateService;
            _blobService = blobService;
        }

        #region Import Candidates
        [HttpPost("import")]
        [CustomAuthorize("Admin", "HR", "Training staff")]
        public async Task<IActionResult> ImportCandidates(IFormFile file)
        {
            var importedByUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                // Sao chép file vào MemoryStream
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // Gọi dịch vụ import
                    ImportResult result = await _candidateService.ImportCandidatesFromExcelAsync(stream, importedByUserId, _blobService);

                    // Xử lý kết quả
                    if (result.Errors.Count > 0)
                    {
                        return BadRequest(new { Message = "Import completed with errors.", Result = result });
                    }

                    return Ok(new { Message = "Import successful.", Result = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Get All Candidates
        [HttpGet]
        [CustomAuthorize("Admin", "HR", "Reviewer", "Training staff")]
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
        [CustomAuthorize("Admin", "HR", "Reviewer", "HeadMaster", "Training staff")]
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

        #region Get Candidates By Request Id
        [HttpGet("candidate/{requestId}")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetCandidatesByRequestId(string requestId)
        {
            try
            {
                var candidates = await _candidateService.GetCandidatesByRequestIdAsync(requestId);
                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Update Candidate
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "HR", "Training staff")]
        public async Task<IActionResult> UpdateCandidate(string id, [FromBody] CandidateUpdateDTO updatedCandidate)
        {
            try
            {
                var candidate = await _candidateService.UpdateCandidateAsync(id, updatedCandidate);
                return Ok(candidate);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Delete Candidate
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "HR")]
        public async Task<IActionResult> DeleteCandidate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID không hợp lệ");
            }

            var (success, message) = await _candidateService.DeleteCandidateAsync(id);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(new { Success = success, Message = message });
        }
        #endregion
    }
}
