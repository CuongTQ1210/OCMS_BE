using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class DepartmentService : IDepartmentService
    {

        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDepartmentRepository _repository;
        public DepartmentService(UnitOfWork unitOfWork, IMapper mapper, IDepartmentRepository repository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _repository = repository;
        }

        #region GetAllDepartmentsAsync
        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            return departments;
        }
        #endregion
        public async Task<bool> AssignUserToDepartmentAsync(string userId, string departmentId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' not found.");

            var department = await _unitOfWork.DecisionRepository.GetByIdAsync(departmentId);
            if (department == null)
                throw new KeyNotFoundException($"Department with ID '{departmentId}' not found.");

            user.DepartmentId = departmentId;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveUserFromDepartmentAsync(string userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' not found.");

            user.DepartmentId = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<DepartmentModel> CreateDepartmentAsync(DepartmentCreateDTO dto)
        {
            // Optional: Check if Specialty exists (to avoid foreign key issues)
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(dto.SpecialtyId);
            if (specialty == null)
            {
                throw new KeyNotFoundException($"Specialty with ID '{dto.SpecialtyId}' not found.");
            }
            var manager = await _unitOfWork.UserRepository.GetByIdAsync(dto.ManagerId);
            if (manager == null)
            {
                throw new KeyNotFoundException($"Manager with ID '{dto.ManagerId}' not found.");
            }
            if(manager.SpecialtyId != dto.SpecialtyId)
            {
                throw new InvalidDataException($"Manager need to have the same specialty as department");
            }
            // Count existing departments with same SpecialtyId to generate unique DepartmentId
            var existingDepartments = await _repository
                .GetDepartmentsBySpecialtyIdAsync(dto.SpecialtyId);

            int sequenceNumber = existingDepartments.Count() + 1;
            string departmentId = $"DE-{dto.SpecialtyId}-{sequenceNumber:D3}";
            manager.DepartmentId = departmentId;
            await _unitOfWork.UserRepository.UpdateAsync(manager);
            var department = new Department
            {
                DepartmentId = departmentId,
                DepartmentName = dto.DepartmentName,
                DepartmentDescription = dto.DepartmentDescription,
                SpecialtyId = dto.SpecialtyId,
                ManagerUserId=dto.ManagerId,
                Specialty= specialty,
                Manager= manager,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = DepartmentStatus.Active,
            };

            await _unitOfWork.DepartmentRepository.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DepartmentModel>(department); 
        }
        #region GetDepartmentByIdAsync
        public async Task<DepartmentModel> GetDepartmentByIdAsync(string departmentId)
        {
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID '{departmentId}' not found.");
            }
            return _mapper.Map<DepartmentModel>(department);
        }
        #endregion

        #region UpdateDepartmentAsync
        public async Task<DepartmentModel> UpdateDepartmentAsync(string departmentId, DepartmentUpdateDTO dto)
        {
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID '{departmentId}' not found.");
            }
            var manager = await _unitOfWork.UserRepository.GetByIdAsync(dto.ManagerId);
            if (manager == null)
            {
                throw new KeyNotFoundException($"Manager with ID '{dto.ManagerId}' not found.");
            }
            if (manager.SpecialtyId != department.SpecialtyId)
            {
                throw new InvalidDataException($"Manager need to have the same specialty as department");
            }
            var oldManager = await _unitOfWork.UserRepository.GetByIdAsync(department.ManagerUserId);
            oldManager.DepartmentId = "";
            await _unitOfWork.UserRepository.UpdateAsync(oldManager);
            manager.DepartmentId = department.DepartmentId;
            await _unitOfWork.UserRepository.UpdateAsync(manager);
            // Update fields
            department.DepartmentName = dto.DepartmentName;
            department.DepartmentDescription = dto.DepartmentDescription;
            department.ManagerUserId = dto.ManagerId;
            department.Manager = manager;
            department.UpdatedAt = DateTime.Now;

            _unitOfWork.DepartmentRepository.UpdateAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DepartmentModel>(department);
        }
        #endregion

        #region DeleteDepartmentAsync
        public async Task<bool> DeleteDepartmentAsync(string departmentId)
        {
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID '{departmentId}' not found.");
            }

            _unitOfWork.DepartmentRepository.DeleteAsync(departmentId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
