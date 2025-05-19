using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassSubjectController : ControllerBase
    {
        private readonly IClassSubjectService _classSubjectService;

        public ClassSubjectController(IClassSubjectService classSubjectService)
        {
            _classSubjectService = classSubjectService;
        }

        #region Get All Class Subjects
        [HttpGet]
        [CustomAuthorize]
        public async Task<IActionResult> GetAllClassSubjects()
        {
            try
            {
                var classSubjects = await _classSubjectService.GetAllClassSubjectsAsync();
                return Ok(new { message = "Class subjects retrieved successfully.", classSubjects });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Class Subject By Id
        [HttpGet("{id}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetClassSubjectById(string id)
        {
            try
            {
                var classSubject = await _classSubjectService.GetClassSubjectByIdAsync(id);
                if (classSubject == null)
                    return NotFound(new { message = $"Class subject with ID {id} not found." });

                return Ok(new { message = "Class subject retrieved successfully.", classSubject });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Class Subject Details
        [HttpGet("details/{id}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetClassSubjectDetails(string id)
        {
            try
            {
                var details = await _classSubjectService.GetClassSubjectDetailsByIdAsync(id);
                if (details == null)
                    return NotFound(new { message = $"Class subject with ID {id} not found." });

                return Ok(new { message = "Class subject details retrieved successfully.", details });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Class Subjects By Class Id
        [HttpGet("class/{classId}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetClassSubjectsByClassId(string classId)
        {
            try
            {
                var classSubjects = await _classSubjectService.GetClassSubjectsByClassIdAsync(classId);
                return Ok(new { message = "Class subjects retrieved successfully.", classSubjects });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Class Subjects By Subject Id
        [HttpGet("subject/{subjectId}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetClassSubjectsBySubjectId(string subjectId)
        {
            try
            {
                var classSubjects = await _classSubjectService.GetClassSubjectsBySubjectIdAsync(subjectId);
                return Ok(new { message = "Class subjects retrieved successfully.", classSubjects });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Class Subjects By Instructor Id
        [HttpGet("instructor/{instructorId}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetClassSubjectsByInstructorId(string instructorId)
        {
            try
            {
                var classSubjects = await _classSubjectService.GetClassSubjectsByInstructorIdAsync(instructorId);
                return Ok(new { message = "Class subjects retrieved successfully.", classSubjects });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Create Class Subject
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateClassSubject([FromBody] ClassSubjectDTO dto)
        {
            try
            {
                var classSubject = await _classSubjectService.CreateClassSubjectAsync(dto);
                return CreatedAtAction(nameof(GetClassSubjectById),
                    new { id = classSubject.ClassSubjectId },
                    new { message = "Class subject created successfully.", classSubject });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Update Class Subject
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateClassSubject(string id, [FromBody] ClassSubjectDTO dto)
        {
            try
            {
                var classSubject = await _classSubjectService.UpdateClassSubjectAsync(id, dto);
                return Ok(new { message = "Class subject updated successfully.", classSubject });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Delete Class Subject
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteClassSubject(string id)
        {
            try
            {
                var result = await _classSubjectService.DeleteClassSubjectAsync(id);
                if (!result)
                    return NotFound(new { message = $"Class subject with ID {id} not found." });

                return Ok(new { message = "Class subject deleted successfully." });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}