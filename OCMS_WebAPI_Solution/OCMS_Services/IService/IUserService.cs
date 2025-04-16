using Microsoft.AspNetCore.Http;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IUserService
    {
        Task<UserModel> GetUserByIdAsync(string id);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<User> CreateUserFromCandidateAsync(string candidateId);
        Task<User> CreateUserAsync(CreateUserDTO userDto);
        Task UpdateUserDetailsAsync(string userId, UserUpdateDTO updateDto);
        Task UpdatePasswordAsync(string userId, PasswordUpdateDTO passwordDto);
        Task ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto);
        Task ResetPasswordAsync(string token, ResetPasswordDTO newPassword);
        Task<IEnumerable<UserModel>> GetUsersByRoleAsync(string roleId);

        Task<string> CreateUserAsync(UserCreateDTO dto);
        Task<string> UpdateUserAvatarAsync(string userId, IFormFile file);
    }
}
