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
    public class CourseSubjectSpecialtyController : ControllerBase
    {
        private readonly ICourseSubjectSpecialtyService _courseSubjectSpecialtyService;
        public CourseSubjectSpecialtyController(ICourseSubjectSpecialtyService courseSubjectSpecialtyService)
        {
            _courseSubjectSpecialtyService = courseSubjectSpecialtyService;
        }

        #region Create Course-Subject-Specialty
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<ActionResult> CreateCourseSubjectSpecialty([FromBody] CourseSubjectSpecialtyDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request data.");
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User ID not found in token");
            }
            
            try
            {
                var createdCourseSubjectSpecialty = await _courseSubjectSpecialtyService.CreateCourseSubjectSpecialtyAsync(dto, currentUserId);
                return Ok(new
                {
                    success = true,
                    message = "Course-Subject-Specialty created successfully.",
                    data = createdCourseSubjectSpecialty
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to create Course-Subject-Specialty.",
                    error = ex.Message
                });
            }
        }
        #endregion
        #region Delete Course-Subject-Specialty
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<ActionResult> DeleteCourseSubjectSpecialty(string id)
        {
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var deletedCourseSubjectSpecialty = await _courseSubjectSpecialtyService.DeleteCourseSubjectSpecialtyAsync(id, currentUserId);
                return Ok(new
                {
                    success = true,
                    message = "Course-Subject-Specialty Delete successfully.",
                    data = deletedCourseSubjectSpecialty
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to delete Course-Subject-Specialty.",
                    error = ex.Message
                });
            }
        }
        #endregion
        #region Delete Subjects by CourseId and SpecialtyId
        [HttpDelete("Allsubject")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<ActionResult> DeleteSubjectsByCourseIdandSpecialtyId([FromBody] DeleteAllSubjectInCourseSpecialty dto)
        {

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var deletedCourseSubjectSpecialty = await _courseSubjectSpecialtyService.DeleteSubjectsbyCourseIdandSpecialtyId(dto, currentUserId);
                return Ok(new
                {
                    success = true,
                    message = "All subjects in the course and specialty have been deleted successfully.",
                    data = deletedCourseSubjectSpecialty
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot delete subject because the course is approved. A request has been sent to the HeadMaster.",
                    error = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Subject not found for the course and specialty.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to delete subject.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Get Subjects by CourseId and SpecialtyId
        [HttpGet("subjects")]
        [CustomAuthorize("Admin", "Training staff", "Instructor", "Trainee")]
        public async Task<ActionResult> GetSubjectsByCourseIdAndSpecialtyId([FromQuery] string courseId, [FromQuery] string specialtyId)
        {
            try
            {
                var subjects = await _courseSubjectSpecialtyService.GetSubjectsByCourseIdAndSpecialtyIdAsync(courseId, specialtyId);
                return Ok(new
                {
                    success = true,
                    message = "Subjects retrieved successfully.",
                    data = subjects
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Subjects not found.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to retrieve subjects.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Get All CourseSubjectSpecialties
        [HttpGet]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<ActionResult> GetAllCourseSubjectSpecialties()
        {
            try
            {
                var courseSubjectSpecialties = await _courseSubjectSpecialtyService.GetAllCourseSubjectSpecialtiesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Course subject specialties retrieved successfully.",
                    data = courseSubjectSpecialties
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to retrieve course subject specialties.",
                    error = ex.Message
                });
            }
        }
        #endregion

    }
}
