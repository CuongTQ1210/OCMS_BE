using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;

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
                return Ok(new { message = "Password updated successfully!" });
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
        public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] string newpass)
        {
            try
            {
                await _userService.ResetPasswordAsync(token,newpass);
                return Ok("Password has been reset.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}

