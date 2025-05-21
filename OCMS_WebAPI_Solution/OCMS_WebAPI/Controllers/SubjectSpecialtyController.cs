using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
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
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> Add([FromBody] SubjectSpecialtyDTO dto)
        {
            var result = await _service.AddAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok();
        }
    }
} 