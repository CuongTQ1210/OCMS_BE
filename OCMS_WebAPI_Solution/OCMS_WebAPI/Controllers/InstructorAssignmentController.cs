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
    public class InstructorAssignmentController : ControllerBase
    {
        private readonly IInstructorAssignmentService _instructorAssignmentService;

        public InstructorAssignmentController(IInstructorAssignmentService instructorAssignmentService)
        {
            _instructorAssignmentService = instructorAssignmentService ?? throw new ArgumentNullException(nameof(instructorAssignmentService));
        }

        /// <summary>
        /// Retrieves all instructor assignments.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllInstructorAssignments()
        {
            var assignments = await _instructorAssignmentService.GetAllInstructorAssignmentsAsync();
            return Ok(new { message = "Instructor assignments retrieved successfully.", data = assignments });
        }

        /// <summary>
        /// Retrieves a specific instructor assignment by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetInstructorAssignmentById(string id)
        {
            try
            {
                var assignment = await _instructorAssignmentService.GetInstructorAssignmentByIdAsync(id);
                return Ok(new { message = "Instructor assignment retrieved successfully.", data = assignment });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new instructor assignment.
        /// </summary>
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateInstructorAssignment([FromBody] InstructorAssignmentDTO dto)
        {
            try
            {
                var assignByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(assignByUserId))
                    return Unauthorized(new { message = "User identity not found." });

                var assignment = await _instructorAssignmentService.CreateInstructorAssignmentAsync(dto, assignByUserId);
                return CreatedAtAction(nameof(GetInstructorAssignmentById), new { id = assignment.AssignmentId },
                    new { message = "Instructor assignment created successfully.", data = assignment });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing instructor assignment.
        /// </summary>
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateInstructorAssignment(string id, [FromBody] InstructorAssignmentDTO dto)
        {
            try
            {
                var assignment = await _instructorAssignmentService.UpdateInstructorAssignmentAsync(id, dto);
                return Ok(new { message = "Instructor assignment updated successfully.", data = assignment });
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
        /// Deletes an instructor assignment by its ID.
        /// </summary>
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteInstructorAssignment(string id)
        {
            try
            {
                bool deleted = await _instructorAssignmentService.DeleteInstructorAssignmentAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Instructor assignment not found." });

                return Ok(new { message = "Instructor assignment deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
