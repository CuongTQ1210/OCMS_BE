using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
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
        public async Task<ActionResult> CreateCourseSubjectSpecialty([FromBody] CourseSubjectSpecialtyDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request data.");
            }
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
            catch (ArgumentException ex)
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
    }
}
