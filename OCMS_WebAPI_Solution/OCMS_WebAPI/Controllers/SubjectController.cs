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
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        #region Get all Subjects
        [HttpGet]
        [CustomAuthorize]
        public async Task<IActionResult> GetAllSubjects()
        {

            try
            {
                var allSubjects = await _subjectService.GetAllSubjectsAsync();

                return Ok(new { message = "Subject deleted successfully.", allSubjects }); 
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
           }
        #endregion

        #region Get Subject By Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSubjectById(string id)
        {
            try
            {
                var subject = await _subjectService.GetSubjectByIdAsync(id);
                return Ok(new { message = "Subject retrieved successfully.",  subject });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        #endregion

        #region Get trainee By SubjectId
        [HttpGet("trainee/{id}")]
        [Authorize]
        public async Task<IActionResult> GetTraineeBySubjectId(string id)
        {
            try
            {
                var trainees = await _subjectService.GetTraineesBySubjectIdAsync(id);
                return Ok(new { message = "Trainee retrieved successfully.", trainees });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Subjects By Course Id
        [HttpGet("course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetSubjectsByCourseId(string courseId)
        {
            try
            {
                var subjects = await _subjectService.GetSubjectsByCourseIdAsync(courseId);
                return Ok(new { message = "Subjects retrieved successfully.", subjects });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Create Subject
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectDTO dto)
        {
            try
            {
                var createdByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var subject = await _subjectService.CreateSubjectAsync(dto, createdByUserId);
                return CreatedAtAction(nameof(GetSubjectById), new { id = subject.SubjectId }, new { message = "Subject created successfully.",  subject });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Update Subject
        [HttpPut("subjectId")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateSubject(string subjectId,[FromBody] SubjectDTO dto)
        {
            
            try
            {
                var subject = await _subjectService.UpdateSubjectAsync(subjectId, dto);
                return Ok(new { message = "Subject updated successfully.",  subject });
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
        #endregion

        #region Delete Subject
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteSubject(string id)
        {
            try
            {
                bool deleted = await _subjectService.DeleteSubjectAsync(id);
                if (!deleted) return NotFound(new { message = "Subject not found." });

                return Ok(new { message = "Subject deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        #endregion
    }
}
