using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System;
using System.Threading.Tasks;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectSpecialtyController : ControllerBase
    {
        private readonly ISubjectSpecialtyService _service;
        public SubjectSpecialtyController(ISubjectSpecialtyService service)
        {
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving subject specialties", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while retrieving subject specialty with id {id}", error = ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> Add([FromBody] SubjectSpecialtyDTO dto)
        {
            try
            {
                var result = await _service.AddAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the subject specialty", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while deleting subject specialty with id {id}", error = ex.Message });
            }
        }
    }
} 