using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OCMS_BOs.ViewModel;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using OCMS_Services.Service;
using OCMS_WebAPI;
using OCMS_WebAPI.AuthorizeSettings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public LoginController(IAuthenticationService authenticationService) 
        {
            _authenticationService = authenticationService;
        }

        #region Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                // Validate input
                if (loginModel == null || string.IsNullOrEmpty(loginModel.Username) || string.IsNullOrEmpty(loginModel.Password))
                {
                    return BadRequest("Username and password are required.");
                }

                var result = await _authenticationService.LoginAsync(loginModel);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        #endregion

    }
}
