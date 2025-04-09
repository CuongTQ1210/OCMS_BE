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
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        #region Get All Grades
        [HttpGet]
        [CustomAuthorize("Admin", "Training staff", "Reviewer", "Instructor")]
        public async Task<IActionResult> GetAllGrades()
        {
            try
            {
                var grades = await _gradeService.GetAllAsync();
                return Ok(grades);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Grade By ID
        [HttpGet("{id}")]
        [CustomAuthorize("Admin", "Training staff", "Reviewer", "Instructor")]
        public async Task<IActionResult> GetGradeById(string id)
        {
            try
            {
                var grade = await _gradeService.GetByIdAsync(id);
                if (grade == null)
                    return NotFound(new { message = "Grade not found." });

                return Ok(grade);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Create Grade
        [HttpPost]
        [CustomAuthorize("Instructor", "Admin")]
        public async Task<IActionResult> CreateGrade([FromBody] GradeDTO dto)
        {
            try
            {
                var gradedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(gradedByUserId))
                    return Unauthorized(new { message = "User identity not found." });

                var newGradeId = await _gradeService.CreateAsync(dto, gradedByUserId);
                return Ok(new { GradeId = newGradeId, message = "Grade created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Update Grade
        [HttpPut("{id}")]
        [CustomAuthorize("Instructor", "Admin")]
        public async Task<IActionResult> UpdateGrade(string id, [FromBody] GradeDTO dto)
        {
            try
            {
                var result = await _gradeService.UpdateAsync(id, dto);
                if (!result)
                    return NotFound(new { message = "Grade not found or update failed." });

                return Ok(new { message = "Grade updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Delete Grade
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff", "Instructor")]
        public async Task<IActionResult> DeleteGrade(string id)
        {
            try
            {
                var result = await _gradeService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Grade not found or deletion failed." });

                return Ok(new { message = "Grade deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
