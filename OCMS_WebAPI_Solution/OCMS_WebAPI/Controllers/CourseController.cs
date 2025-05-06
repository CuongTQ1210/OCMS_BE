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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        #region Create Course
        /// <summary>
        /// Create a new course
        /// </summary>
        [HttpPost("create")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseDTO dto)
        {
            try
            {
                var createdByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(createdByUserId))
                    return Unauthorized(new { success = false, message = "Unauthorized access." });

                var createdCourse = await _courseService.CreateCourseAsync(dto, createdByUserId);
                return Ok(new
                {
                    success = true,
                    message = "Course created successfully.",
                    data = createdCourse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to create course.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Get All Courses
        /// <summary>
        /// Get all courses
        /// </summary>
        [HttpGet("all")]
        [Authorize()]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Courses retrieved successfully.",
                    data = courses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to retrieve courses.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Get Course By ID
        /// <summary>
        /// Get a course by ID
        /// </summary>
        [HttpGet("{id}")]

        public async Task<IActionResult> GetCourseById(string id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound(new { success = false, message = "Course not found." });

                return Ok(new
                {
                    success = true,
                    message = "Course retrieved successfully.",
                    data = course
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to retrieve course.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Assign Course to Training Plan
        [HttpPost("assign")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> AssignCourseToTrainingPlan(string courseId, string trainingPlanId)
        {
            try
            {
                var result = await _courseService.AssignCourseToTrainingPlanAsync(courseId, trainingPlanId);
                return Ok(new
                {
                    success = true,
                    message = "Course assigned to training plan successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to assign course to training plan.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Update Course
        /// <summary>
        /// Update a course
        /// </summary>
        [HttpPut("update/{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateCourse(string id, [FromBody] CourseUpdateDTO dto)
        {
            try
            {
                var updatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(updatedByUserId))
                    return Unauthorized(new { success = false, message = "Unauthorized access." });

                var updatedCourse = await _courseService.UpdateCourseAsync(id, dto, updatedByUserId);
                if (updatedCourse == null)
                    return NotFound(new { success = false, message = "Course not found." });

                return Ok(new
                {
                    success = true,
                    message = "Course updated successfully.",
                    data = updatedCourse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to update course.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Delete Course
        /// <summary>
        /// Delete a course
        /// </summary>
        [HttpDelete("delete/{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteCourse(string id)
        {
            try
            {
                var result = await _courseService.DeleteCourseAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Course not found." });

                return Ok(new
                {
                    success = true,
                    message = "Course deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to delete course.",
                    error = ex.Message
                });
            }
        }
        #endregion
    }
}
