using OCMS_BOs.Helper;
using OCMS_BOs.ViewModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCMS_Services.IService;
using AutoMapper;
using OCMS_Repositories;
using System.IO;
using System.Security.Cryptography;
using OfficeOpenXml;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace OCMS_Services.Service
{
    public class UserService : IUserService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IEmailService _emailService;

        public UserService(UnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
            _emailService = emailService;
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
            return _mapper.Map<UserModel>(user);
        }
        #endregion

        #region Create User using Candidate
        public async Task<User> CreateUserFromCandidateAsync(string candidateId)
        {
            var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(candidateId);
            if (candidate == null) throw new Exception("Candidate not found.");

            // Lấy Specialty
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(candidate.SpecialtyId);
            string specialtyInitial = specialty.SpecialtyName.Substring(0, 1).ToUpper();

            // Sinh userId
            string lastUserId = await GetLastUserIdAsync();
            int nextNumber = lastUserId != null ? int.Parse(lastUserId.Substring(1)) + 1 : 1;
            string userId = $"{specialtyInitial}{nextNumber:D6}";

            // Tạo userName
            string lastName = candidate.FullName.Split(' ').Last().ToLower();
            string userName = $"{lastName}_{userId.ToLower()}";

            // Tạo password ngẫu nhiên
            string password = GenerateRandomPassword();

            var user = new User
            {
                UserId = userId,
                Username = userName,
                PasswordHash = _passwordHasher.HashPassword(null, password),
                FullName = candidate.FullName,
                Email = candidate.Email,
                RoleId = 7, // Role mặc định cho User thông thường
                SpecialtyId = candidate.SpecialtyId,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email
            await SendWelcomeEmailAsync(candidate.Email, userName, password);

            return user;
        }

        private async Task<string> GetLastUserIdAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            return users
                .Select(u => u.UserId)
                .Where(id => Regex.IsMatch(id, @"^[A-Z]\d{6}$"))
                .OrderByDescending(id => id)
                .FirstOrDefault();
        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task SendWelcomeEmailAsync(string email, string username, string password)
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
        #endregion
    }
}