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

        public UserService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
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
    }
}