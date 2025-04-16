using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        #region Get All Users
        [HttpGet]
        [CustomAuthorize]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get User Profile
        [HttpGet("profile")]
        [CustomAuthorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identity not found." });

            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                return Ok(new { message = "User profile fetched successfully!", user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Get User By Id
        [HttpGet("{id}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(new { message = "User got successfully!", user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region upload avatar 
        [HttpPut("avatar")]
        [CustomAuthorize]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identity not found." });

            try
            {
                var avatarUrl = await _userService.UpdateUserAvatarAsync(userId, file);
                return Ok(new { message = "Avatar updated successfully!", avatarUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Create User from Candidate
        [HttpPost("create-from-candidate/{candidateId}")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> CreateUserFromCandidate(string candidateId)
        {
            try
            {
                var user = await _userService.CreateUserFromCandidateAsync(candidateId);
                return Ok(new { message = "User created successfully!", user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Create User
        [HttpPost]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO userDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(userDto);
                return Ok(new { message = "User created successfully!", user });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Update User Details
        [HttpPut("{id}/details")]
        [CustomAuthorize]
        public async Task<IActionResult> UpdateUserDetails(string id, [FromBody] UserUpdateDTO updateDto)
        {
            try
            {
                await _userService.UpdateUserDetailsAsync(id, updateDto);
                var user = _userService.GetUserByIdAsync(id);
                 return Ok(new { message = "User updated successfully!", user });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Update Password
        [HttpPut("{id}/password")]
        [CustomAuthorize]
        public async Task<IActionResult> UpdatePassword(string id, [FromBody] PasswordUpdateDTO passwordDto)
        {
            try
            {
                await _userService.UpdatePasswordAsync(id, passwordDto);
                return Ok(new { message = "Password updated successfully!"});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Forgot Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
        {
            try
            {
                await _userService.ForgotPasswordAsync(forgotPasswordDto);
                return Ok("Password reset link sent.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Reset Password
        [HttpPost("reset-password/{token}")]
        public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordDTO newPassword)
        {
            try
            {
                await _userService.ResetPasswordAsync(token, newPassword);
                return Ok("Password has been reset.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion


        [HttpPost]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = await _userService.CreateUserAsync(dto);
                return Ok(new { UserId = userId, Message = "User created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("checktime")]
        public IActionResult CheckTime()
        {
            return Ok(new
            {
                LocalTime = DateTime.Now,
                UtcTime = DateTime.UtcNow,
                OffsetUtcNow = DateTimeOffset.UtcNow,
                OffsetNow = DateTimeOffset.Now
            });
        }
    }
}

