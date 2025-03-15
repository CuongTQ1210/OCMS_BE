using OCMS_BOs.Helper;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly JWTTokenHelper _jwtTokenHelper;

        public AuthenticationService(IUserRepository userRepository, JWTTokenHelper jwtTokenHelper)
        {
            _userRepository = userRepository;
            _jwtTokenHelper = jwtTokenHelper;
        }

        public async Task<LoginResModel> LoginAsync(LoginModel loginDto)
        {
            // Find the user by their username and include the Role entity
            var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username);

            // Check if the user exists and the password is correct
            if (user == null || !PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password.");
            }

            // Ensure that the user role is loaded, if it's not already, load it.
            var roles = new List<string> { user.Role?.RoleName ?? "" }; // Default to "User" if no role is assigned

            // Generate JWT token
            var token = _jwtTokenHelper.GenerateToken(user, roles);

            // Return the response DTO with the user details and token
            return new LoginResModel
            {
                UserID = user.UserId,
                Token = token
            };
        }
    }
}
