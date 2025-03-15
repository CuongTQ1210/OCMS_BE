using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.Entities;
using OCMS_BOs.ViewModel;
using OCMS_Services.IService;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialtyController : ControllerBase
    {
        private readonly ISpecialtyService _specialtyService;
        public SpecialtyController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        #region Get All Specialties
        [HttpGet]
        public async Task<IActionResult> GetAllSpecialties()
        {
            try
            {
                var specialties = await _specialtyService.GetAllSpecialtiesAsync();
                return Ok(specialties);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get Specialty By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecialtyById(string id)
        {
            try
            {
                var specialty = await _specialtyService.GetSpecialtyByIdAsync(id);
                return Ok(specialty);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Add Specialty
        [HttpPost]
        public async Task<IActionResult> AddSpecialty([FromBody] SpecialtyModel specialty)
        {
            var createdByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var result = await _specialtyService.AddSpecialtyAsync(specialty, createdByUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Delete Specialty
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialty(string id)
        {
            try
            {
                var result = await _specialtyService.DeleteSpecialtyAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Update Specialty
        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateSpecialty(string id, [FromBody] SpecialtyModel specialty)
        {
            var updatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var result = await _specialtyService.UpdateSpecialtyAsync(id, specialty, updatedByUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
