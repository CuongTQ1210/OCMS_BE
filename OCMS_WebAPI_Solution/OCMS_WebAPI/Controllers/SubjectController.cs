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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _subjectService.GetAllSubjectsAsync();
            return Ok(new { message = "Subjects retrieved successfully.",  subjects });
        }

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

        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateSubject(string id, [FromBody] SubjectDTO dto)
        {
            
            try
            {
                var subject = await _subjectService.UpdateSubjectAsync(id, dto);
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
    }
}
