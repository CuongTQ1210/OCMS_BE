using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        #region Get All Classes
        [HttpGet]
        [CustomAuthorize]
        public async Task<IActionResult> GetAllClasses()
        {
            try
            {
                var classes = await _classService.GetAllClassesAsync();
                return Ok(new { message = "Classes retrieved successfully.", classes });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Class By Id
        [HttpGet("{id}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetClassById(string id)
        {
            try
            {
                var classModel = await _classService.GetClassByIdAsync(id);
                return Ok(new { message = "Class retrieved successfully.", class_data = classModel });
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

        #region Create Class
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateClass([FromBody] ClassDTO dto)
        {
            try
            {
                var classModel = await _classService.CreateClassAsync(dto);
                return CreatedAtAction(nameof(GetClassById), new { id = classModel.ClassId },
                    new { message = "Class created successfully.", class_data = classModel });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
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

        #region Update Class
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateClass(string id, [FromBody] ClassDTO dto)
        {
            try
            {
                var classModel = await _classService.UpdateClassAsync(id, dto);
                return Ok(new { message = "Class updated successfully.", class_data = classModel });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Delete Class
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> DeleteClass(string id)
        {
            try
            {
                var result = await _classService.DeleteClassAsync(id);
                return Ok(new { message = "Class deleted successfully." });
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
    }
}