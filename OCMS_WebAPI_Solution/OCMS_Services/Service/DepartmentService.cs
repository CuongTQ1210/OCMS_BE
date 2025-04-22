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
        public async Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync()
        {
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DepartmentModel>>(departments);
        }
        #endregion
        public async Task<bool> AssignUserToDepartmentAsync(string userId, string departmentId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' not found.");

            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)
                throw new KeyNotFoundException($"Department with ID '{departmentId}' not found.");

            if (department.Status == DepartmentStatus.Inactive)
                throw new InvalidOperationException($"Cannot assign user to an inactive department (ID: '{departmentId}').");
            if(user.SpecialtyId !=department.SpecialtyId)
                throw new InvalidOperationException($"Cannot assign user (SpecialtyId : {user.SpecialtyId} to this department (ID: '{departmentId}') with SpecialtyId {department.SpecialtyId}");
            user.DepartmentId = departmentId;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveUserFromDepartmentAsync(string userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' not found.");

            if (string.IsNullOrEmpty(user.DepartmentId))
                throw new InvalidOperationException("User is not currently assigned to any department.");

            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(user.DepartmentId);
            if (department == null)
                throw new KeyNotFoundException($"Department with ID '{user.DepartmentId}' not found.");

            if (department.Status == DepartmentStatus.Inactive)
                throw new InvalidOperationException($"Cannot remove user from an inactive department (ID: '{department.DepartmentId}').");

            user.DepartmentId = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<DepartmentModel> CreateDepartmentAsync(DepartmentCreateDTO dto)
        {
            // 1. Validate Specialty
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(dto.SpecialtyId);
            if (specialty == null)
                throw new KeyNotFoundException($"Specialty with ID '{dto.SpecialtyId}' not found.");

            // 2. Validate Manager (optional)
            User manager = null;
            if (!string.IsNullOrWhiteSpace(dto.ManagerId))
            {
                manager = await _unitOfWork.UserRepository.GetByIdAsync(dto.ManagerId);
                if (manager == null)
                    throw new KeyNotFoundException($"Manager with ID '{dto.ManagerId}' not found.");
                if (!string.IsNullOrEmpty(manager.DepartmentId))
                {
                    throw new InvalidOperationException($"Manager '{manager.UserId}' is already assigned to Department '{manager.DepartmentId}'.");
                }
                if (manager.SpecialtyId != dto.SpecialtyId)
                    throw new InvalidDataException("Manager must have the same specialty as the department.");
            }

            // 3. Generate new DepartmentId
            var existingDepartments = await _repository.GetDepartmentsBySpecialtyIdAsync(dto.SpecialtyId);
            int sequenceNumber = existingDepartments.Count() + 1;
            string departmentId = $"DE-{dto.SpecialtyId}-{sequenceNumber:D3}";

            // 4. Create department entity
            var department = new Department
            {
                DepartmentId = departmentId,
                DepartmentName = dto.DepartmentName,
                DepartmentDescription = dto.DepartmentDescription,
                SpecialtyId = dto.SpecialtyId,
                ManagerUserId = manager?.UserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = DepartmentStatus.Active,
            };

            // 5. Add department to DB
            await _unitOfWork.DepartmentRepository.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            // 6. Update manager's DepartmentId
            if (manager != null)
            {
                manager.DepartmentId = departmentId;
                await _unitOfWork.UserRepository.UpdateAsync(manager);
                await _unitOfWork.SaveChangesAsync();
            }

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
            if (dto.ManagerId == "")
            {
                throw new InvalidDataException("ManagerId can not be null!!");
            }
            if (!string.IsNullOrEmpty(dto.ManagerId))
            {
                var manager = await _unitOfWork.UserRepository.GetByIdAsync(dto.ManagerId);
                if (manager == null)
                {
                    throw new KeyNotFoundException($"Manager with ID '{dto.ManagerId}' not found.");
                }

                if (manager.SpecialtyId != department.SpecialtyId)
                {
                    throw new InvalidDataException($"Manager must have the same specialty as the department.");
                }

                if (!string.IsNullOrEmpty(manager.DepartmentId))
                {
                    throw new InvalidOperationException($"Manager '{manager.UserId}' is already assigned to Department '{manager.DepartmentId}'.");
                }

                // Clear old manager
                if (department.ManagerUserId != dto.ManagerId)
                {
                    var oldManager = await _unitOfWork.UserRepository.GetByIdAsync(department.ManagerUserId);
                    if (oldManager != null)
                    {
                        oldManager.DepartmentId = null;
                        await _unitOfWork.UserRepository.UpdateAsync(oldManager);
                    }

                    // Assign new manager
                    manager.DepartmentId = departmentId;
                    await _unitOfWork.UserRepository.UpdateAsync(manager);

                    department.ManagerUserId = dto.ManagerId;
                    department.Manager = manager;
                }
            }
            else
            {
                // If user clears manager ID
                var oldManager = await _unitOfWork.UserRepository.GetByIdAsync(department.ManagerUserId);
                if (oldManager != null)
                {
                    oldManager.DepartmentId = null;
                    await _unitOfWork.UserRepository.UpdateAsync(oldManager);
                }

                department.ManagerUserId = null;
                department.Manager = null;
            }

            department.DepartmentName = dto.DepartmentName;
            department.DepartmentDescription = dto.DepartmentDescription;
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
            if (department.Status == DepartmentStatus.Inactive)
                throw new InvalidOperationException("Department is already deactivated.");
            department.Status = DepartmentStatus.Inactive;
            department.UpdatedAt = DateTime.Now;

            await _unitOfWork.DepartmentRepository.UpdateAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion
        #region active department
        public async Task<bool> ActivateDepartmentAsync(string departmentId)
        {
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID '{departmentId}' not found.");
            }

            if (department.Status == DepartmentStatus.Active)
                throw new InvalidOperationException("Department is already activated.");

            department.Status = DepartmentStatus.Active;
            department.UpdatedAt = DateTime.Now;

            await _unitOfWork.DepartmentRepository.UpdateAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion
    }
}
