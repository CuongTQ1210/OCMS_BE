using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.Entities;
using OCMS_Services.IService;

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
        public async Task<IActionResult> AddSpecialty([FromBody]Specialties specialty)
        {
            try
            {
                var result = await _specialtyService.AddSpecialtyAsync(specialty);
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
        [HttpPut]
        public async Task<IActionResult> UpdateSpecialty(string id)
        {
            try
            {
                var result = await _specialtyService.UpdateSpecialtyAsync(id);
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
