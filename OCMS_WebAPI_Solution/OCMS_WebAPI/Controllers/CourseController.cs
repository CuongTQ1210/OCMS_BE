using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;
using OCMS_BOs.ResponseModel;

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


        [HttpPost("send-request/{courseId}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> SendCourseRequest(string courseId)
        {
            // Get user ID from JWT or context
            string userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            try
            {
                var result = await _courseService.SendCourseRequestForApprove(courseId, userId);
                if (result)
                    return Ok("Course request submitted successfully.");
                return BadRequest("Failed to submit course request.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

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

        #region Import Courses
        /// <summary>
        /// Import courses from Excel file
        /// </summary>
        [HttpPost("import")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> ImportCourses(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded." });

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { success = false, message = "Only .xlsx files are supported." });

                var importedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(importedByUserId))
                    return Unauthorized(new { success = false, message = "Unauthorized access." });

                using var stream = file.OpenReadStream();
                var result = await _courseService.ImportCoursesAsync(stream, importedByUserId);

                return Ok(new
                {
                    success = true,
                    message = $"Import completed. Successfully imported {result.SuccessCount} courses. Failed to import {result.FailedCount} courses.",
                    data = new
                    {
                        successCount = result.SuccessCount,
                        failureCount = result.FailedCount,
                        errors = result.Errors
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to import courses.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Assign SubjectSpecialty
        /// <summary>
        /// Assign a SubjectSpecialty to a course
        /// </summary>
        [HttpPost("assign-subject-specialty")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> AssignSubjectSpecialty([FromBody] AssignSubjectSpecialtyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CourseId) || string.IsNullOrEmpty(request.SubjectSpecialtyId))
                    return BadRequest(new { success = false, message = "Course ID and SubjectSpecialty ID are required." });

                var updatedCourse = await _courseService.AssignSubjectSpecialtyAsync(request.CourseId, request.SubjectSpecialtyId);
                return Ok(new
                {
                    success = true,
                    message = "SubjectSpecialty assigned successfully.",
                    data = updatedCourse
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to assign SubjectSpecialty.",
                    error = ex.Message
                });
            }
        }
        #endregion
    }
}
