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
    public class DecisonController : ControllerBase
    {
        private readonly IDecisionService _decisionService;
        public DecisonController(IDecisionService decisionService)
        {
            _decisionService = decisionService;
        }

        #region Create Decision
        [HttpPost("CreateDecision")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateDecision([FromBody] CreateDecisionDTO dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _decisionService.CreateDecisionForCourseAsync(dto, userId);
            if (result == null)
            {
                return BadRequest("Failed to create decision");
            }
            return Ok(result);
        }
        #endregion

        #region Get All Draft Decisions
        [HttpGet("GetAllDraftDecisions")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetAllDraftDecisions()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            var result = await _decisionService.GetAllDraftDecisionsAsync();
            if (result == null)
            {
                return NotFound("No draft decisions found");
            }
            return Ok(result);
        }
        #endregion

        #region Get All Signed Decisions
        [HttpGet("GetAllApprovedDecisions")]
        [CustomAuthorize("Admin", "Training staff", "HeadMaster")]
        public async Task<IActionResult> GetAllApprovedDecisions()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            var result = await _decisionService.GetAllSignDecisionsAsync();
            if (result == null)
            {
                return NotFound("No approved decisions found");
            }
            return Ok(result);
        }
        #endregion

        #region Delete Decision
        [HttpDelete("DeleteDecision/{decisionId}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteDecision(string decisionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            var result = await _decisionService.DeleteDecisionAsync(decisionId);
            if (!result)
            {
                return NotFound("Decision not found or could not be deleted");
            }
            return Ok("Decision deleted successfully");
        }
        #endregion
    }
}
