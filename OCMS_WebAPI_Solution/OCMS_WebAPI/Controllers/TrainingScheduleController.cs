using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_Services.Service;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingScheduleController : ControllerBase
    {
        private readonly ITrainingScheduleService _trainingScheduleService;

        public TrainingScheduleController(ITrainingScheduleService trainingScheduleService)
        {
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
        }

        /// <summary>
        /// Retrieves all training schedules.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllTrainingSchedules()
        {
            var schedules = await _trainingScheduleService.GetAllTrainingSchedulesAsync();
            return Ok(new { message = "Training schedules retrieved successfully.",  schedules });
        }

        /// <summary>
        /// Retrieves a specific training schedule by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTrainingScheduleById(string id)
        {
            try
            {
                var schedule = await _trainingScheduleService.GetTrainingScheduleByIdAsync(id);
                return Ok(new { message = "Training schedule retrieved successfully.", schedule });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new training schedule.
        /// </summary>
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateTrainingSchedule([FromBody] TrainingScheduleDTO dto)
        {
            try
            {
                var createdByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(createdByUserId))
                    return Unauthorized(new { message = "User identity not found." });

                var schedule = await _trainingScheduleService.CreateTrainingScheduleAsync(dto, createdByUserId);
                return CreatedAtAction(nameof(GetTrainingScheduleById), new { id = schedule.ScheduleID },
                    new { message = "Training schedule created successfully.", data = schedule });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing training schedule.
        /// </summary>
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateTrainingSchedule(string id, [FromBody] TrainingScheduleDTO dto)
        {
            try
            {
                var schedule = await _trainingScheduleService.UpdateTrainingScheduleAsync(id, dto);
                return Ok(new { message = "Training schedule updated successfully.", schedule });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a training schedule by its ID.
        /// </summary>
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteTrainingSchedule(string id)
        {
            try
            {
                bool deleted = await _trainingScheduleService.DeleteTrainingScheduleAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Training schedule not found." });

                return Ok(new { message = "Training schedule deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("instructor/subjects")]
        [CustomAuthorize("Instructor")]
        public async Task<IActionResult> GetSubjectsAndSchedules()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized(new { message = "Instructor ID not found in token." });

            try
            {
                var subscheldule = await _trainingScheduleService.GetSubjectsAndSchedulesForInstructorAsync(instructorId);
                return Ok(new { message = "Joined Subjects And Schedules retrieved successfully.", subscheldule });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
