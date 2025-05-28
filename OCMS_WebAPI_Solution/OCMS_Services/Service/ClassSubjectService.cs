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
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class ClassSubjectService : IClassSubjectService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClassSubjectRepository _classSubjectRepository;

        public ClassSubjectService(UnitOfWork unitOfWork, IMapper mapper, IClassSubjectRepository classSubjectRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _classSubjectRepository = classSubjectRepository ?? throw new ArgumentNullException(nameof(classSubjectRepository));
        }

        #region Create Class Subject
        public async Task<ClassSubjectModel> CreateClassSubjectAsync(ClassSubjectDTO dto)
        {
            // Validate inputs
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Check if class exists and get CourseId
            var classEntity = await _unitOfWork.Context.Set<Class>()
                .Include(c => c.Course)
                    .ThenInclude(course => course.SubjectSpecialties)
                .FirstOrDefaultAsync(c => c.ClassId == dto.ClassId);
            if (classEntity == null)
                throw new KeyNotFoundException($"Class with ID {dto.ClassId} does not exist.");

            // Check if subject specialty exists
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetByIdAsync(dto.SubjectSpecialtyId);
            if (subjectSpecialty == null)
                throw new KeyNotFoundException($"SubjectSpecialty with ID {dto.SubjectSpecialtyId} does not exist.");

            // Check if SubjectSpecialty belongs to the Course of the Class
            if (!classEntity.Course.SubjectSpecialties.Any(ss => ss.SubjectSpecialtyId == dto.SubjectSpecialtyId))
                throw new InvalidOperationException($"SubjectSpecialty {dto.SubjectSpecialtyId} does not belong to Course {classEntity.CourseId} of Class {dto.ClassId}.");

            // Check if instructor assignment exists
            var instructorAssignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(dto.InstructorAssignmentID);
            if (instructorAssignment == null)
                throw new KeyNotFoundException($"Instructor assignment with ID {dto.InstructorAssignmentID} does not exist.");
            if (instructorAssignment.RequestStatus != RequestStatus.Approved)
                throw new InvalidOperationException($"Instructor assignment hasn't been approved yet.");
            if (instructorAssignment.SubjectId != subjectSpecialty.SubjectId)
                throw new InvalidOperationException($"Instructor with id {instructorAssignment.InstructorId} can't teach {subjectSpecialty.SubjectId}.");

            // Check if class-subject combination already exists
            bool exists = await _unitOfWork.ClassSubjectRepository.AnyAsync(
                cs => cs.ClassId == dto.ClassId && cs.SubjectSpecialtyId == dto.SubjectSpecialtyId
            );
            if (exists)
                throw new InvalidOperationException($"A class-subject relationship with Class ID {dto.ClassId} and SubjectSpecialty ID {dto.SubjectSpecialtyId} already exists.");

            // Create new ClassSubject entity
            var classSubject = new ClassSubject
            {
                ClassSubjectId = await GenerateClassSubjectIdAsync(),
                ClassId = dto.ClassId,
                SubjectSpecialtyId = dto.SubjectSpecialtyId,
                SubjectSpecialty = subjectSpecialty,
                InstructorAssignmentID = dto.InstructorAssignmentID
            };

            // Add to repository
            await _unitOfWork.ClassSubjectRepository.AddAsync(classSubject);

            // Create and return the model
            return await GetClassSubjectByIdAsync(classSubject.ClassSubjectId);
        }
        #endregion

        #region Get Class Subject by ID
        public async Task<ClassSubjectModel> GetClassSubjectByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(id);
            if (classSubject == null)
                return null;

            // L?y c�c entity li�n quan
            var classEntity = await _unitOfWork.Context.Set<Class>().FindAsync(classSubject.ClassId);
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetByIdAsync(classSubject.SubjectSpecialtyId);
            var subject = subjectSpecialty != null ?
                await _unitOfWork.SubjectRepository.GetByIdAsync(subjectSpecialty.SubjectId) : null;
            var instructorAssignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(classSubject.InstructorAssignmentID);
            var instructor = instructorAssignment != null ?
                await _unitOfWork.UserRepository.GetByIdAsync(instructorAssignment.InstructorId) : null;

            // T?o model th? c�ng v� g�n gi� tr? tr?c ti?p thay v� d�ng AutoMapper
            var model = new ClassSubjectModel
            {
                ClassSubjectId = classSubject.ClassSubjectId,
                ClassId = classSubject.ClassId,
                ClassName = classEntity?.ClassName,
                SubjectSpecialtyId = classSubject.SubjectSpecialtyId,
                InstructorAssignmentID = classSubject.InstructorAssignmentID,
            };

            return model;
        }
        #endregion

        #region Get All Class Subjects
        public async Task<IEnumerable<ClassSubjectModel>> GetAllClassSubjectsAsync()
        {
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync();
            var models = new List<ClassSubjectModel>();

            foreach (var classSubject in classSubjects)
            {
                var model = await GetClassSubjectByIdAsync(classSubject.ClassSubjectId);
                if (model != null)
                    models.Add(model);
            }

            return models;
        }
        #endregion

        #region Get Class Subjects by Class ID
        public async Task<IEnumerable<ClassSubjectModel>> GetClassSubjectsByClassIdAsync(string classId)
        {
            if (string.IsNullOrEmpty(classId))
                throw new ArgumentNullException(nameof(classId));

            var classSubjects = await _classSubjectRepository.GetClassSubjectsByClassIdAsync(classId);
            return await MapClassSubjectsToModelsAsync(classSubjects);
        }
        #endregion

        #region Get Class Subjects by subject specialty ID
        public async Task<IEnumerable<ClassSubjectModel>> GetClassSubjectsBySubjectIdAsync(string subjectSpecialtyId)
        {
            if (string.IsNullOrEmpty(subjectSpecialtyId))
                throw new ArgumentNullException(nameof(subjectSpecialtyId));

            var classSubjects = await _classSubjectRepository.GetClassSubjectsBySubjectSpecialtyIdAsync(subjectSpecialtyId);
            return await MapClassSubjectsToModelsAsync(classSubjects);
        }
        #endregion

        #region Get Class Subjects by Instructor ID
        public async Task<IEnumerable<ClassSubjectModel>> GetClassSubjectsByInstructorIdAsync(string instructorId)
        {
            if (string.IsNullOrEmpty(instructorId))
                throw new ArgumentNullException(nameof(instructorId));

            var classSubjects = await _classSubjectRepository.GetClassSubjectsByInstructorIdAsync(instructorId);
            return await MapClassSubjectsToModelsAsync(classSubjects);
        }
        #endregion

        #region Update Class Subject
        public async Task<ClassSubjectModel> UpdateClassSubjectAsync(string id, ClassSubjectDTO dto)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Get the existing entity
            var existingClassSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(id);
            if (existingClassSubject == null)
                throw new KeyNotFoundException($"ClassSubject with ID {id} not found.");

            // Check if class exists and get CourseId
            var classEntity = await _unitOfWork.Context.Set<Class>()
                .Include(c => c.Course)
                    .ThenInclude(course => course.SubjectSpecialties)
                .FirstOrDefaultAsync(c => c.ClassId == dto.ClassId);
            if (classEntity == null)
                throw new KeyNotFoundException($"Class with ID {dto.ClassId} does not exist.");

            // Check if subject specialty exists
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetByIdAsync(dto.SubjectSpecialtyId);
            if (subjectSpecialty == null)
                throw new KeyNotFoundException($"SubjectSpecialty with ID {dto.SubjectSpecialtyId} does not exist.");

            // Check if SubjectSpecialty belongs to the Course of the Class
            if (!classEntity.Course.SubjectSpecialties.Any(ss => ss.SubjectSpecialtyId == dto.SubjectSpecialtyId))
                throw new InvalidOperationException($"SubjectSpecialty {dto.SubjectSpecialtyId} does not belong to Course {classEntity.CourseId} of Class {dto.ClassId}.");

            // Check if instructor assignment exists
            var instructorAssignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(dto.InstructorAssignmentID);
            if (instructorAssignment == null)
                throw new KeyNotFoundException($"Instructor assignment with ID {dto.InstructorAssignmentID} does not exist.");

            // Check if class-subject specialty combination already exists
            if (existingClassSubject.ClassId != dto.ClassId || existingClassSubject.SubjectSpecialtyId != dto.SubjectSpecialtyId)
            {
                bool exists = await _unitOfWork.ClassSubjectRepository.AnyAsync(
                    cs => cs.ClassId == dto.ClassId &&
                          cs.SubjectSpecialtyId == dto.SubjectSpecialtyId &&
                          cs.ClassSubjectId != id
                );
                if (exists)
                    throw new InvalidOperationException($"A class-subject relationship with Class ID {dto.ClassId} and SubjectSpecialty ID {dto.SubjectSpecialtyId} already exists.");
            }

            // Update entity using AutoMapper
            _mapper.Map(dto, existingClassSubject);
            await _unitOfWork.ClassSubjectRepository.UpdateAsync(existingClassSubject);

            // Return updated model
            return await GetClassSubjectByIdAsync(id);
        }
        #endregion

        #region Delete Class Subject
        public async Task<bool> DeleteClassSubjectAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            // Get the entity to check if it exists
            var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(id);
            if (classSubject == null)
                return false;

            // Delete the entity
            await _unitOfWork.ClassSubjectRepository.DeleteAsync(id);
            return true;
        }
        #endregion

        #region Get Class Subject Details
        public async Task<ClassSubjectDetailModel> GetClassSubjectDetailsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var classSubject = await _classSubjectRepository.GetClassSubjectWithDetailsByIdAsync(id);
            if (classSubject == null)
                return null;

            // Use AutoMapper to map from entity to detail model
            var detailModel = _mapper.Map<ClassSubjectDetailModel>(classSubject);

            // Calculate any additional statistics
            if (classSubject.traineeAssigns != null)
            {
                detailModel.SetEnrolledTraineesCount(classSubject.traineeAssigns.Count); // Use the new method
            }

            return detailModel;
        }
        #endregion

        #region Helper Methods
        private async Task<string> GenerateClassSubjectIdAsync()
        {
            string prefix = "CSB";

            // Get the last ID to determine the next sequence number
            var lastClassSubject = await _unitOfWork.ClassSubjectRepository.GetLastObjectIdAsync(
                cs => cs.ClassSubjectId.StartsWith(prefix),
                cs => cs.ClassSubjectId
            );

            int sequenceNumber = 1;

            if (lastClassSubject != null)
            {
                string lastId = lastClassSubject.ClassSubjectId;
                string sequenceStr = lastId.Substring(prefix.Length);
                if (int.TryParse(sequenceStr, out int lastSequence))
                {
                    sequenceNumber = lastSequence + 1;
                }
            }

            return $"{prefix}{sequenceNumber:D6}";
        }

        private async Task<IEnumerable<ClassSubjectModel>> MapClassSubjectsToModelsAsync(IEnumerable<ClassSubject> classSubjects)
        {
            var models = new List<ClassSubjectModel>();

            foreach (var classSubject in classSubjects)
            {
                var model = await GetClassSubjectByIdAsync(classSubject.ClassSubjectId);
                if (model != null)
                    models.Add(model);
            }

            return models;
        }
        #endregion
    }
}