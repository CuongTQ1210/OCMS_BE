using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class CourseSubjectSpecialtyService : ICourseSubjectSpecialtyService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly ITrainingScheduleService _trainingScheduleService;

        public CourseSubjectSpecialtyService(UnitOfWork unitOfWork, IMapper mapper,
            IRequestService requestService, ITrainingScheduleService trainingScheduleService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
        }

        #region Create Course-Subject-Specialty
        public async Task<CourseSubjectSpecialtyModel> CreateCourseSubjectSpecialtyAsync(CourseSubjectSpecialtyDTO dto, string createdByUserId)
        {
            // Validate CourseId, SubjectId, SpecialtyId
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseId);
            if (course == null)
                throw new ArgumentException($"Course with ID '{dto.CourseId}' does not exist.");

            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(dto.SubjectId);
            if (subject == null)
                throw new ArgumentException($"Subject with ID '{dto.SubjectId}' does not exist.");

            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(dto.SpecialtyId);
            if (specialty == null)
                throw new ArgumentException($"Specialty with ID '{dto.SpecialtyId}' does not exist.");

            // Validate user
            if (!await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == createdByUserId))
                throw new ArgumentException("The specified User ID does not exist.");

            // Check if Course is approved
            if (course.Status == CourseStatus.Approved)
            {
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.Update,
                    RequestEntityId = $"{dto.CourseId}:{dto.SubjectId}:{dto.SpecialtyId}",
                    Description = $"Request to add subject {dto.SubjectId} to course {dto.CourseId} with specialty {dto.SpecialtyId}",
                    Notes = "Awaiting HeadMaster approval"
                };
                await _requestService.CreateRequestAsync(requestDto, createdByUserId);
                throw new InvalidOperationException("Cannot add subject to an approved course. A request has been sent to the HeadMaster.");
            }

            // Check for existing relationship
            if (await _unitOfWork.CourseSubjectSpecialtyRepository.ExistsAsync(
                css => css.CourseId == dto.CourseId && css.SubjectId == dto.SubjectId && css.SpecialtyId == dto.SpecialtyId))
                throw new ArgumentException("This course-subject-specialty relationship already exists.");

            // Map DTO to entity
            var css = _mapper.Map<CourseSubjectSpecialty>(dto);
            css.Id = Guid.NewGuid().ToString();
            css.CreatedByUserId = createdByUserId;
            css.CreatedAt = DateTime.UtcNow;
            css.UpdatedAt = DateTime.UtcNow;

            // Add to repository
            await _unitOfWork.CourseSubjectSpecialtyRepository.AddAsync(css);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseSubjectSpecialtyModel>(css);
        }
        #endregion

        #region Delete Course-Subject-Specialty
        public async Task<bool> DeleteCourseSubjectSpecialtyAsync(string id, string deletedByUserId)
        {
            var css = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAsync(
                css => css.Id == id,
                css => css.Course
            );
            if (css == null)
                throw new KeyNotFoundException("CourseSubjectSpecialty not found.");

            // Check if Course is approved
            if (css.Course.Status == CourseStatus.Approved)
            {
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.Delete,
                    RequestEntityId = id,
                    Description = $"Request to delete course-subject-specialty {id}",
                    Notes = "Awaiting HeadMaster approval"
                };
                await _requestService.CreateRequestAsync(requestDto, deletedByUserId);
                throw new InvalidOperationException($"Cannot delete course-subject-specialty {id} because the course is Approved. A request has been sent to the HeadMaster.");
            }

            // Delete related schedules
            var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(s => s.CourseSubjectSpecialtyId == id);
            foreach (var schedule in schedules)
            {
                await _trainingScheduleService.DeleteTrainingScheduleAsync(schedule.ScheduleID);
            }

            // Delete CourseSubjectSpecialty
            await _unitOfWork.CourseSubjectSpecialtyRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion
    }
}
