﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        private readonly IBlobService _blobService;

        public UserService(UnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IConnectionMultiplexer redis, IUserRepository userRepository, IBlobService blobService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _redis = redis.GetDatabase();
            _userRepository = userRepository;
            _blobService = blobService;
        }

        #region Get All Users
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserModel>>(users);
        }
        #endregion

        #region Get User By Id
        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("User not found!!");
            }
            var userModel = _mapper.Map<UserModel>(user);
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                userModel.AvatarUrlWithSas = await _blobService.GetBlobUrlWithSasTokenAsync(
                    user.AvatarUrl, TimeSpan.FromHours(1));
            }
            return userModel;
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
            var specialtyPrefix = GetSpecialtyPrefix(specialty.SpecialtyName);

            // Tạo userId với format mới
            string userId = await GenerateNextUserIdAsync(specialtyPrefix);

            // Tạo userName
            string fullNameWithoutDiacritics = RemoveDiacritics(candidate.FullName);
            string lastName = fullNameWithoutDiacritics.Split(' ').Last().ToLower();

            string baseUserName = $"{lastName}{userId}";
            string userName = baseUserName;

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
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Address = candidate.Address,
                Gender = candidate.Gender,
                DateOfBirth = candidate.DateOfBirth,
                PhoneNumber = candidate.PhoneNumber,
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email
            await SendWelcomeEmailAsync(candidate.Email, userName, password);

            return user;
        }
        #endregion

        #region Create User
        public async Task<User> CreateUserAsync(CreateUserDTO userDto)
        {
            if (!Regex.IsMatch(userDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Invalid email format.");

            // Ánh xạ DTO sang User entity
            var user = _mapper.Map<User>(userDto);

            // Xử lý tạo UserId với format mới
            string specialtyPrefix = "US"; // Default prefix
            if (!string.IsNullOrEmpty(user.SpecialtyId))
            {
                var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(user.SpecialtyId);
                if (specialty != null)
                {
                    specialtyPrefix = GetSpecialtyPrefix(specialty.SpecialtyName);
                }
            }
            string userId = await GenerateNextUserIdAsync(specialtyPrefix);
            user.UserId = userId;

            // Tạo Username từ họ tên (loại bỏ dấu)
            string fullNameWithoutDiacritics = RemoveDiacritics(user.FullName);
            string lastName = fullNameWithoutDiacritics.Split(' ').Last().ToLower();
            string userName = $"{lastName}{userId.ToLower()}";

            // Đảm bảo Username là duy nhất
            int usernameSuffix = 1;
            string originalUserName = userName;
            while (await IsUsernameExists(userName))
            {
                userName = $"{originalUserName}_{usernameSuffix}";
                usernameSuffix++;
            }
            user.Username = userName;
            var departmentid = userDto.DepartmentId ?? null;
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (userDto.RoleId == 8)
                {

                    var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentid);
                    if (department.ManagerUserId != null)
                        throw new Exception("Department already has a manager.");
                    department.ManagerUserId = userId;
                    user.DepartmentId = departmentid;
                }
            }
            else
            {
                user.DepartmentId = null;
            }
            // Tạo Password tự động (8 ký tự với chữ hoa, chữ thường, số và ký tự đặc biệt)
            string password = GenerateRandomPassword();
            user.PasswordHash = PasswordHasher.HashPassword(password);

            // Thiết lập các thông tin khác
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            // Thiết lập Role cho User (nếu không có trong DTO)
            if (user.RoleId == 0)
            {
                // Mặc định là Training Staff
                var trainingStaffRole = await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "Training Staff");
                user.RoleId = trainingStaffRole?.RoleId ?? 1;
            }

            // Lưu người dùng mới
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email chào mừng với thông tin đăng nhập
            await SendWelcomeEmailAsync(user.Email, user.Username, password);

            return user;
        }
        #endregion

        #region deactivate user
        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");
            if (user.Status == AccountStatus.Deactivated)
                throw new InvalidOperationException("User is already deactivated.");
            user.Status = AccountStatus.Deactivated; // or false if it's a boolean field

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region activate user
        public async Task<bool> ActivateUserAsync(string userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");
            if (user.Status == AccountStatus.Active)
                throw new InvalidOperationException("User is already active.");
            user.Status = AccountStatus.Active; // or true if it's a boolean

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Update User Details
        public async Task UpdateUserDetailsAsync(string userId, UserUpdateDTO updateDto)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            _mapper.Map(updateDto, user);
            user.UpdatedAt = DateTime.Now;

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
            user.UpdatedAt = DateTime.Now;

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

            var baseUrl = "https://ocms-teal.vercel.app";
            var resetLink = $"{baseUrl}/reset-password/{token}";
            string emailBody = $"Click the following link to reset your password: {resetLink}";

            await _emailService.SendEmailAsync(user.Email, "Password Reset", emailBody);
        }
        #endregion

        #region Reset Password
        public async Task ResetPasswordAsync(string token, ResetPasswordDTO newPassword)
        {
            string userId = await _redis.StringGetAsync(token);
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Invalid or expired token.");

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");
            if (newPassword == null)
            {
                throw new Exception("New Password can not be empty.");
            }
            user.PasswordHash = PasswordHasher.HashPassword(newPassword.NewPassword);
            user.UpdatedAt = DateTime.Now;

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

        /// <summary>
        /// Tạo UserId tiếp theo với format: prefix của specialty + 3 số tăng dần bắt đầu từ 1
        /// VD: GO001, GO002, IT001, IT002...
        /// </summary>
        /// <param name="specialtyPrefix">Prefix của specialty (VD: GO, IT, HR...)</param>
        /// <returns>UserId mới</returns>
        private async Task<string> GenerateNextUserIdAsync(string specialtyPrefix)
        {
            // Lấy tất cả UserId có cùng prefix
            var userIds = await _unitOfWork.UserRepository
                .GetQuery()
                .Where(u => u.UserId.StartsWith(specialtyPrefix))
                .Select(u => u.UserId)
                .ToListAsync();

            int maxNumber = 0;

            // Tìm số lớn nhất trong các UserId có cùng prefix
            foreach (var userId in userIds)
            {
                if (userId.Length == specialtyPrefix.Length + 6) // Format: XX000 (prefix + 3 số)
                {
                    string numericPart = userId.Substring(specialtyPrefix.Length); // Lấy 3 số cuối
                    if (int.TryParse(numericPart, out int number))
                    {
                        maxNumber = Math.Max(maxNumber, number);
                    }
                }
            }

            // Tạo UserId mới với số tiếp theo
            int nextNumber = maxNumber + 1;
            return $"{specialtyPrefix}{nextNumber:D6}"; // Format: GO001, GO002...
        }

        /// <summary>
        /// Lấy prefix cho specialty từ tên specialty
        /// VD: "Ground Operation" → "GO", "Information Technology" → "IT"
        /// </summary>
        /// <param name="specialtyName">Tên specialty</param>
        /// <returns>Prefix 2 ký tự</returns>
        private string GetSpecialtyPrefix(string specialtyName)
        {
            if (string.IsNullOrWhiteSpace(specialtyName))
                return "US"; // Default prefix

            // Tách các từ và lấy chữ cái đầu
            var words = specialtyName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length >= 2)
            {
                // Lấy chữ cái đầu của 2 từ đầu tiên
                return $"{words[0][0]}{words[1][0]}".ToUpper();
            }
            else if (words.Length == 1)
            {
                // Nếu chỉ có 1 từ, lấy 2 ký tự đầu
                return words[0].Length >= 2 ?
                    words[0].Substring(0, 2).ToUpper() :
                    words[0].ToUpper().PadRight(2, 'X');
            }

            return "US"; // Default prefix
        }

        private async Task<bool> IsUsernameExists(string username)
        {
            return await _unitOfWork.UserRepository.ExistsAsync(u => u.Username == username);
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

        #region upload avatar
        public async Task<string> UpdateUserAvatarAsync(string userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                await _blobService.DeleteFileAsync(user.AvatarUrl);
            }

            // Upload new file
            var blobName = $"{Guid.NewGuid()}_{file.FileName}";
            var containerName = "avatars";

            await using var stream = file.OpenReadStream();
            var avatarUrl = await _blobService.UploadFileAsync(containerName, blobName, stream, "image/jpeg");

            // Lưu URL gốc (không có SAS token) vào database
            user.AvatarUrl = _blobService.GetBlobUrlWithoutSasToken(avatarUrl);
            user.UpdatedAt = DateTime.Now;

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Trả về URL có SAS token cho client
            return await _blobService.GetBlobUrlWithSasTokenAsync(user.AvatarUrl, TimeSpan.FromHours(1));
        }
        #endregion
    }
}