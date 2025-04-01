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

        #region Get All Specialties
        [HttpGet]
        [CustomAuthorize]
        public async Task<IActionResult> GetAllSpecialties()
        {
            try
            {
                var specialties = await _specialtyService.GetAllSpecialtiesAsync();
                var response = new GetAllSpecialtiesResponse
                {
                    Data = specialties.ToList()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = $"Error retrieving specialties: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
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
                var response = new GetSpecialtyTreeResponse
                {
                    Data = specialtyTree.ToList()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = $"Error retrieving specialty tree: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
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
                    var notFoundResponse = new ErrorResponse
                    {
                        Message = $"Specialty with ID {id} not found."
                    };
                    return NotFound(notFoundResponse);
                }

                var response = new GetSpecialtyResponse
                {
                    Data = specialty
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = $"Error retrieving specialty: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }
        #endregion

        #region Add Specialty
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff", "AOC Manager")]
        public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var validationErrors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    var errorResponse = new ErrorResponse
                    {
                        Message = validationErrors
                    };
                    return BadRequest(errorResponse);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    var errorResponse = new ErrorResponse
                    {
                        Message = "User ID not found in token."
                    };
                    return BadRequest(errorResponse);
                }

                var specialty = await _specialtyService.AddSpecialtyAsync(model, userId);
                var response = new CreateSpecialtyResponse
                {
                    Data = specialty
                };
                return CreatedAtAction(nameof(GetSpecialtyById), new { id = specialty.SpecialtyId }, response);
            }
            catch (ArgumentException ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = ex.Message
                };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = $"Error creating specialty: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
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
                var response = new DeleteSpecialtyResponse
                {
                    SpecialtyId = id
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    var notFoundResponse = new ErrorResponse
                    {
                        Message = ex.Message
                    };
                    return NotFound(notFoundResponse);
                }

                var errorResponse = new ErrorResponse
                {
                    Message = ex.Message
                };
                return BadRequest(errorResponse);
            }
            catch (InvalidOperationException ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = ex.Message
                };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = $"Error deleting specialty: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }
        #endregion

        #region Update Specialty
        [HttpPut("{id}")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> UpdateSpecialty(string id, [FromBody] UpdateSpecialtyDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var validationErrors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    var errorResponse = new ErrorResponse
                    {
                        Message = validationErrors
                    };
                    return BadRequest(errorResponse);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    var errorResponse = new ErrorResponse
                    {
                        Message = "User ID not found in token."
                    };
                    return BadRequest(errorResponse);
                }

                var specialty = await _specialtyService.UpdateSpecialtyAsync(id, model, userId);
                var response = new UpdateSpecialtyResponse
                {
                    Data = specialty
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    var notFoundResponse = new ErrorResponse
                    {
                        Message = ex.Message
                    };
                    return NotFound(notFoundResponse);
                }

                var errorResponse = new ErrorResponse
                {
                    Message = ex.Message
                };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = $"Error updating specialty: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }
        #endregion
    }
}
