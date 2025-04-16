using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_Services.Service;
using OCMS_WebAPI.AuthorizeSettings;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // GET: api/department/{id}
        [HttpGet("{id}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetDepartmentById(string id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                return Ok(department);
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

        // POST: api/department
        [HttpPost]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentCreateDTO dto)
        {
            try
            {
                var created = await _departmentService.CreateDepartmentAsync(dto);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = created.DepartmentId }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/department/{id}
        [HttpPut("{id}")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> UpdateDepartment(string id, [FromBody] DepartmentUpdateDTO dto)
        {
            try
            {
                var updated = await _departmentService.UpdateDepartmentAsync(id, dto);
                return Ok(updated);
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
        [HttpPut("assign-to-department/{departmentId}/{userId}")]
        [CustomAuthorize("Admin","HR", "AOC Manager")]
        public async Task<IActionResult> AddUserToDepartment(string departmentId, string userId)
        {
            var result = await _departmentService.AssignUserToDepartmentAsync(userId, departmentId);

            if (!result)
                return NotFound(new { message = "User not found or already removed from department." });

            return Ok(new { message = "User add to department successfully." });
        }
        [HttpPut("remove-from-department/{userId}")]
        [CustomAuthorize("Admin","HR","AOC Manager")]
        public async Task<IActionResult> RemoveUserFromDepartment(string userId)
        {
            var result = await _departmentService.RemoveUserFromDepartmentAsync(userId);

            if (!result)
                return NotFound(new { message = "User not found or already removed from department." });

            return Ok(new { message = "User removed from department successfully." });
        }
        // DELETE: api/department/{id}
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> DeleteDepartment(string id)
        {
            try
            {
                var result = await _departmentService.DeleteDepartmentAsync(id);
                return result ? Ok(new { message = "Deleted successfully." }) : NotFound();
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
    }
}
