using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_Services.Service;
using OCMS_WebAPI.AuthorizeSettings;
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
        [HttpGet("edu-officer/requests")]
        [CustomAuthorize("Training staff")]
        public async Task<IActionResult> GetRequestForEduOfficer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            try
            {
                var requests = await _requestService.GetRequestsForEducationOfficerAsync();

                if (requests == null || !requests.Any())
                    return NotFound(new { message = "No relevant requests found." });

                return Ok(new
                {
                    message = "Requests retrieved successfully.",
                    requests
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }
        // ✅ Get All Requests (Only for Admin & Director)
        [HttpGet]
        [CustomAuthorize("Admin", "HeadMaster")]
        public async Task<IActionResult> GetAllRequests()
        {

            var requests = await _requestService.GetAllRequestsAsync();
            return Ok(requests);
        }

        // ✅ Delete Request (Only for Admin)
        [HttpDelete("{id}")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> DeleteRequest(string id)
        {

            var success = await _requestService.DeleteRequestAsync(id);
            if (!success)
                return NotFound("Request not found");

            return NoContent();
        }

        // ✅ Approve Request (Only for Director)
        [HttpPut("{id}/approve")]
        [CustomAuthorize("HeadMaster", "Training staff")]
        public async Task<IActionResult> ApproveRequest(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var success = await _requestService.ApproveRequestAsync(id, userId);
            if (!success)
                return NotFound("Request not found");

            return Ok("Request approved successfully");
        }

        // ✅ Reject Request (Only for HeadMaster)
        [HttpPut("{id}/reject")]
        [CustomAuthorize("HeadMaster", "Training staff")]
        public async Task<IActionResult> RejectRequest(string id, [FromBody] RejectRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _requestService.RejectRequestAsync(id, dto.RejectReason, userId);
            if (!success)
                return NotFound("Request not found");

            return Ok("Request rejected successfully");
        }
    }
}
