using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.Helper;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using StackExchange.Redis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OCMS_Services.Service
{
    public class UserService : IUserService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IDatabaseAsync _redis;
        private readonly IUserRepository _userRepository;

        public UserService(UnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IConnectionMultiplexer redis, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _redis = redis.GetDatabase();
            _userRepository = userRepository;
        }

        #region Get All Users
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserModel>>(users);
        }
        #endregion

        #region Get User By Id
        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("User not found!!");
            }
                return _mapper.Map<UserModel>(user);
        }
        #endregion

        #region Create User using Candidate
        public async Task<User> CreateUserFromCandidateAsync(string candidateId)
        {
            var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(candidateId);
            if (candidate == null) throw new Exception("Candidate not found.");

            if (candidate.CandidateStatus != CandidateStatus.Approved)
                throw new Exception("Candidate must be approved first.");
            // Lấy Specialty
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(candidate.SpecialtyId);
            string specialtyInitial = specialty.SpecialtyId;

            // Tạo userId
            string lastUserId = await GetLastUserIdAsync();
            int nextNumber = lastUserId != null ? int.Parse(lastUserId.Substring(1)) + 1 : 1;
            string userId = $"{specialtyInitial}{nextNumber:D6}";
            while (await IsUserIdExists(userId))
            {
                nextNumber++;
                userId = $"{specialtyInitial}{nextNumber:D6}";
            }

            // Tạo userName
            string fullNameWithoutDiacritics = RemoveDiacritics(candidate.FullName);
            string lastName = fullNameWithoutDiacritics.Split(' ').Last().ToLower();
            string userName = $"{lastName}_{userId.ToLower()}";

            // Tạo password ngẫu nhiên
            string password = GenerateRandomPassword();

            var user = new User
            {
                UserId = userId,
                Username = userName,
                PasswordHash = PasswordHasher.HashPassword(password),
                FullName = candidate.FullName,
                Email = candidate.Email,
                RoleId = 7, // Role mặc định cho User thông thường
                SpecialtyId = candidate.SpecialtyId,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Address= candidate.Address,
                Gender= candidate.Gender,
                DateOfBirth= candidate.DateOfBirth,
                PhoneNumber= candidate.PhoneNumber,
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email
            await SendWelcomeEmailAsync(candidate.Email, userName, password);

            return user;
        }
        #endregion

        #region Update User Details
        public async Task UpdateUserDetailsAsync(string userId, UserUpdateDTO updateDto)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            _mapper.Map(updateDto, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Update Password
        public async Task UpdatePasswordAsync(string userId, PasswordUpdateDTO passwordDto)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (!PasswordHasher.VerifyPassword(passwordDto.CurrentPassword, user.PasswordHash))
                throw new Exception("Current password is incorrect.");

            user.PasswordHash = PasswordHasher.HashPassword(passwordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Forgot Password
        public async Task ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto)
        {
            var users = await _unitOfWork.UserRepository.FindAsync(u => u.Email == forgotPasswordDto.Email);
            if (users == null || !users.Any())
                throw new Exception("User not found.");

            var user = users.First();
            string token = Guid.NewGuid().ToString();

            // Store token in Redis with 15-minute expiration
            await _redis.StringSetAsync(token, user.UserId, TimeSpan.FromMinutes(15));

            var baseUrl = "https://ocms-vjvn.azurewebsites.net"; // Có thể lấy từ cấu hình
            var resetLink = $"{baseUrl}/reset-password?token={token}";
            string emailBody = $"Click the following link to reset your password: {resetLink}";

            await _emailService.SendEmailAsync(user.Email, "Password Reset", emailBody);
        }
        #endregion

        #region Reset Password
        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            string userId = await _redis.StringGetAsync(token);
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Invalid or expired token.");

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate the token
            await _redis.KeyDeleteAsync(token);
        }
        #endregion

        #region Get Users By Role
        public async Task<IEnumerable<UserModel>> GetUsersByRoleAsync(string roleId)
        {
            var users = await _userRepository.GetUsersByRoleAsync(roleId);
            if (users == null || !users.Any())
                throw new Exception("No users found for this role.");
            return _mapper.Map<IEnumerable<UserModel>>(users);
        }
        #endregion

        #region Helper Methods
        private async Task SendWelcomeEmailAsync(string email, string username, string password)
        {
            var subject = "Chào mừng bạn đến với hệ thống!";
            var body = $@"Chào mừng bạn đến với hệ thống OCMS!

                        Tài khoản của bạn đã được tạo thành công.

Thông tin đăng nhập:
- Username: {username}
- Password: {password}

Vui lòng đăng nhập và reset mật khẩu của bạn ngay sau khi nhận được email này.

Trân trọng,
Đội ngũ hỗ trợ";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        private async Task<string> GetLastUserIdAsync()
        {
            var userIds = await _unitOfWork.UserRepository
                .GetQuery()
                .Select(u => u.UserId)
                .Where(id => Regex.IsMatch(id, @"^[A-Z]+\d{6}$"))
                .OrderByDescending(id => id)
                .ToListAsync(); // ✅ Chuyển sang List

            return userIds.FirstOrDefault(); // ✅ Lấy phần tử đầu tiên từ List
        }
        private async Task<bool> IsUserIdExists(string userId)
        {
            return await _unitOfWork.UserRepository.GetQuery().AnyAsync(u => u.UserId == userId);
        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }        
        #endregion
    }

}