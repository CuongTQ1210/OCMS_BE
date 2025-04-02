using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
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

        // Helper method để kiểm tra ModelState
        private IActionResult ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponse { Message = validationErrors });
            }
            return null;
        }

        // Helper method để lấy UserId từ Claims
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        #region Get All Specialties
        [HttpGet]
        [CustomAuthorize]
        public async Task<IActionResult> GetAllSpecialties()
        {
            try
            {
                var specialties = await _specialtyService.GetAllSpecialtiesAsync();
                return Ok(new GetAllSpecialtiesResponse { Data = specialties.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"Error retrieving specialties: {ex.Message}" });
            }
        }
        #endregion

        #region Get Specialty Tree
        [HttpGet("tree")]
        [CustomAuthorize]
        public async Task<IActionResult> GetSpecialtyTree()
        {
            try
            {
                var specialtyTree = await _specialtyService.GetSpecialtyTreeAsync();
                return Ok(new GetSpecialtyTreeResponse { Data = specialtyTree.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"Error retrieving specialty tree: {ex.Message}" });
            }
        }
        #endregion

        #region Get Specialty By Id
        [HttpGet("{id}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetSpecialtyById(string id)
        {
            try
            {
                var specialty = await _specialtyService.GetSpecialtyByIdAsync(id);
                if (specialty == null)
                {
                    return NotFound(new ErrorResponse { Message = $"Specialty with ID {id} not found." });
                }
                return Ok(new GetSpecialtyResponse { Data = specialty });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"Error retrieving specialty: {ex.Message}" });
            }
        }
        #endregion

        #region Add Specialty
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff", "AOC Manager")]
        public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyDTO model)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ErrorResponse { Message = "User ID not found in token." });
            }

            try
            {
                var specialty = await _specialtyService.AddSpecialtyAsync(model, userId);
                return CreatedAtAction(nameof(GetSpecialtyById), new { id = specialty.SpecialtyId },
                    new CreateSpecialtyResponse { Data = specialty });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"Error creating specialty: {ex.Message}" });
            }
        }
        #endregion

        #region Delete Specialty
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> DeleteSpecialty(string id)
        {
            try
            {
                await _specialtyService.DeleteSpecialtyAsync(id);
                return Ok(new DeleteSpecialtyResponse { SpecialtyId = id });
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new ErrorResponse { Message = ex.Message });
                }
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"Error deleting specialty: {ex.Message}" });
            }
        }
        #endregion

        #region Update Specialty
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateSpecialty(string id, [FromBody] UpdateSpecialtyDTO model)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ErrorResponse { Message = "User ID not found in token." });
            }

            try
            {
                var specialty = await _specialtyService.UpdateSpecialtyAsync(id, model, userId);
                return Ok(new UpdateSpecialtyResponse { Data = specialty });
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new ErrorResponse { Message = ex.Message });
                }
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"Error updating specialty: {ex.Message}" });
            }
        }
        #endregion
    }
}