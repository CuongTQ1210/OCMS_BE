using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingPlanController : ControllerBase
    {
        private readonly ITrainingPlanService _trainingPlanService;

        public TrainingPlanController(ITrainingPlanService trainingPlanService)
        {
            _trainingPlanService = trainingPlanService;
        }

        #region Create Training Plan
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateTrainingPlan([FromBody] TrainingPlanDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid input data." });

            var createdByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var createdPlan = await _trainingPlanService.CreateTrainingPlanAsync(dto, createdByUserId);
                return Ok(new { message = "Training plan created successfully.", data = createdPlan });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get All Training Plans
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllTrainingPlans()
        {
            try
            {
                var plans = await _trainingPlanService.GetAllTrainingPlansAsync();
                return Ok(new { message = "Training plans retrieved successfully.", data = plans });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Training Plan By Id
        [HttpGet("{id}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetTrainingPlanById(string id)
        {
            try
            {
                var plan = await _trainingPlanService.GetTrainingPlanByIdAsync(id);
                if (plan == null)
                    return NotFound(new { message = "Training plan not found." });

                return Ok(new { message = "Training plan retrieved successfully.", data = plan });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Update Training Plan
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateTrainingPlan(string id, [FromBody] TrainingPlanDTO dto)
        {
            try
            {
                var updatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var updatedPlan = await _trainingPlanService.UpdateTrainingPlanAsync(id, dto, updatedByUserId);

                return Ok(new { message = "Training plan updated successfully.", data = updatedPlan });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Training plan not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Delete Training Plan
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteTrainingPlan(string id)
        {
            try
            {
                var result = await _trainingPlanService.DeleteTrainingPlanAsync(id);
                if (!result)
                    return NotFound(new { message = "Training plan not found." });

                return Ok(new { message = "Training plan deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
