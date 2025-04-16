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
    }
}
