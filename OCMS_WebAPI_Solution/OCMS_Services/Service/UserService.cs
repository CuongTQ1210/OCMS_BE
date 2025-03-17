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
using System.Globalization;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace OCMS_Services.Service
{
    public class UserService : IUserService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UserService(UnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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