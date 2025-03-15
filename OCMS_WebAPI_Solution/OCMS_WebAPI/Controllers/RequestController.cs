using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_Services.Service;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;
        public RequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateRequest([FromBody] RequestDTO requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extract UserId from the JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID is missing in the token" });

            try
            {
                // Create request
                var createdRequest = await _requestService.CreateRequestAsync(requestDto, userId);

                // Return Created (201) response
                return CreatedAtAction(nameof(GetRequest), new { id = createdRequest.RequestId }, createdRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the request", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetRequest(string id)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
                return NotFound();

            return Ok(request);
        }
        // ✅ Get All Requests (Only for Admin & Director)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllRequests()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin" && userRole != "Director")
                return Forbid("Only Admin and Director can view all requests");

            var requests = await _requestService.GetAllRequestsAsync();
            return Ok(requests);
        }

        // ✅ Delete Request (Only for Admin)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRequest(string id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
                return Forbid("Only Admin can delete requests");

            var success = await _requestService.DeleteRequestAsync(id);
            if (!success)
                return NotFound("Request not found");

            return NoContent();
        }

        // ✅ Approve Request (Only for Director)
        [HttpPut("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveRequest(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Director")
                return Forbid("Only Director can approve requests");

            var success = await _requestService.ApproveRequestAsync(id, userId);
            if (!success)
                return NotFound("Request not found");

            return Ok("Request approved successfully");
        }

        // ✅ Reject Request (Only for Director)
        [HttpPut("{id}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectRequest(string id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Director")
                return Forbid("Only Director can reject requests");

            var success = await _requestService.RejectRequestAsync(id);
            if (!success)
                return NotFound("Request not found");

            return Ok("Request rejected successfully");
        }
    }
}
