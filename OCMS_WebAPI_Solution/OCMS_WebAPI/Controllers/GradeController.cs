using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_Services.IService;
using OCMS_Services.Service;
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
        #region Import Grade Trainee
        [HttpPost("import")]
        [CustomAuthorize("Admin", "Instructor")]
        public async Task<IActionResult> ImportGradeTrainees(IFormFile file)
        {
            var importedByUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    ImportResult result = await _gradeService.ImportGradesFromExcelAsync(stream, importedByUserId);

                    if (result.Errors.Count > 0)
                    {
                        return Ok(new { Message = "Import completed with errors.", Result = result });
                    }

                    return Ok(new { Message = "Import successful.", Result = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion
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
        #region Get Passed Grades
        [HttpGet("passed")]
        [CustomAuthorize("Admin", "Training staff", "Reviewer")]
        public async Task<IActionResult> GetPassedGrades()
        {
            try
            {
                var grades = await _gradeService.GetGradesByStatusAsync(GradeStatus.Pass);
            return Ok(grades);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Failed Grades
        [HttpGet("failed")]
        [CustomAuthorize("Admin", "Training staff", "Reviewer")]
        public async Task<IActionResult> GetFailedGrades()
        {
            try
                {
                    var grades = await _gradeService.GetGradesByStatusAsync(GradeStatus.Fail);
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
