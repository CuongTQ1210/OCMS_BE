using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class ClassService : IClassService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClassRepository _classRepository;

        public ClassService(UnitOfWork unitOfWork, IMapper mapper, IClassRepository classRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
        }

        #region Create Class
        public async Task<ClassModel> CreateClassAsync(ClassDTO dto)
        {
            if (string.IsNullOrEmpty(dto.ClassName))
                throw new ArgumentException("Class name cannot be empty.");

            // Check if class with the same name already exists
            var classWithSameName = await _unitOfWork.ClassRepository.GetFirstOrDefaultAsync(c => c.ClassName == dto.ClassName);
            if (classWithSameName != null)
            {
                throw new InvalidOperationException($"Class with name '{dto.ClassName}' already exists.");
            }

            // Generate a new ClassId
            var classId = await GenerateClassId();

            // Create the class entity
            var classEntity = new Class
            {
                ClassId = classId,
                ClassName = dto.ClassName,
                CourseId = dto.CourseId,
            };

            await _unitOfWork.ClassRepository.AddAsync(classEntity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassModel>(classEntity);
        }
        #endregion

        #region Delete Class
        public async Task<bool> DeleteClassAsync(string id)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(id);
            if (classEntity == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            // Check if class has any class subjects
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(cs => cs.ClassId == id);
            if (classSubjects.Any())
                throw new InvalidOperationException("Cannot delete a class that has subjects assigned. Remove the class subjects first.");

            await _unitOfWork.ClassRepository.DeleteAsync(id);
            return true;
        }
        #endregion

        #region Get All Classes
        public async Task<IEnumerable<ClassModel>> GetAllClassesAsync()
        {
            var classes = await _unitOfWork.ClassRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ClassModel>>(classes);
        }
        #endregion

        #region Get Class By Id
        public async Task<ClassModel> GetClassByIdAsync(string id)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(id);
            if (classEntity == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            return _mapper.Map<ClassModel>(classEntity);
        }
        #endregion

        #region Update Class
        public async Task<ClassModel> UpdateClassAsync(string id, ClassDTO dto)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(id);
            if (classEntity == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (string.IsNullOrEmpty(dto.ClassName))
                throw new ArgumentException("Class name cannot be empty.");

            classEntity.ClassName = dto.ClassName;

            await _unitOfWork.ClassRepository.UpdateAsync(classEntity);

            return _mapper.Map<ClassModel>(classEntity);
        }
        #endregion

        #region Helper Methods
        private async Task<string> GenerateClassId()
        {
            const string prefix = "CL";
            const int digits = 3;

            // Get all existing class IDs
            var classes = await _unitOfWork.ClassRepository.GetAllAsync();

            // Default to 1 if no classes exist
            int highestNumber = 0;

            foreach (var classEntity in classes)
            {
                if (classEntity.ClassId.StartsWith(prefix) &&
                    classEntity.ClassId.Length == prefix.Length + digits &&
                    int.TryParse(classEntity.ClassId.Substring(prefix.Length), out int currentNumber))
                {
                    highestNumber = Math.Max(highestNumber, currentNumber);
                }
            }

            // Increment to get the next number
            int nextNumber = highestNumber + 1;

            // Format with leading zeros to maintain 3 digits
            return $"{prefix}{nextNumber.ToString($"D{digits}")}";
        }
        #endregion
    }
}
