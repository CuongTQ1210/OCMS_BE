using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using Org.BouncyCastle.Utilities;
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
            // Check for that specialty has already have subject
            if (course.CourseLevel == CourseLevel.Initial)
            {
                if (await _unitOfWork.CourseSubjectSpecialtyRepository.ExistsAsync(
                    css => css.SubjectId == dto.SubjectId && css.SpecialtyId == dto.SpecialtyId))
                    throw new ArgumentException("Specialty: " + dto.SpecialtyId + " has already exist Subject: " + dto.SubjectId);
            }
            else
            {
                if (await _unitOfWork.CourseSubjectSpecialtyRepository.ExistsAsync(
                 css => css.SubjectId == dto.SubjectId && css.SpecialtyId == dto.SpecialtyId && dto.CourseId == css.CourseId))
                    throw new ArgumentException("Course: " + dto.CourseId +" for Specialty: " + dto.SpecialtyId + " has already exist Subject: " + dto.SubjectId);

            }
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

        #region Delete All Subjects by Course and Specialty 
        public async Task<bool> DeleteSubjectsbyCourseIdandSpecialtyId(DeleteAllSubjectInCourseSpecialty dto, string deletedByUserId)
        {
            var css = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.SpecialtyId == dto.SpecialtyId && css.CourseId == dto.CourseId,
                css => css.Course
            );
            if (css == null || !css.Any())
                throw new KeyNotFoundException("CourseSubjectSpecialty not found.");

            // Check if Course is approved
            if (css[0].Course.Status == CourseStatus.Approved)
            {
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.Delete,
                    RequestEntityId =$"{dto.CourseId}:{dto.SpecialtyId}",
                    Description = $"Request to delete all subject for Course: {dto.CourseId} - Specialty: {dto.SpecialtyId}",
                    Notes = "Awaiting HeadMaster approval"
                };
                await _requestService.CreateRequestAsync(requestDto, deletedByUserId);
                throw new InvalidOperationException($"Cannot delete all subject for Course: {dto.CourseId} - Specialty: {dto.SpecialtyId} because the course is Approved. A request has been sent to the HeadMaster.");
            }
            
            foreach (var c in css)
            {
                // Delete related schedules
                var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(s => s.CourseSubjectSpecialtyId == c.Id);
                foreach (var schedule in schedules)
                {
                    await _trainingScheduleService.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                }

                // Delete CourseSubjectSpecialty
                await _unitOfWork.CourseSubjectSpecialtyRepository.DeleteAsync(c.Id);
            }
            
            // Gọi SaveChangesAsync một lần sau khi đã xóa tất cả
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        #endregion

        #region Get Subjects By CourseId And SpecialtyId
        public async Task<List<SubjectModel>> GetSubjectsByCourseIdAndSpecialtyIdAsync(string courseId, string specialtyId)
        {
            // check if the course and specialty exist
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID '{courseId}' does not exist.");

            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(specialtyId);
            if (specialty == null)
                throw new KeyNotFoundException($"Specialty with ID '{specialtyId}' does not exist.");

            // get the list of CourseSubjectSpecialty by courseId and specialtyId
            var cssList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.CourseId == courseId && css.SpecialtyId == specialtyId,
                css => css.Subject
            );

            if (!cssList.Any())
                throw new KeyNotFoundException($"No subjects found for course '{courseId}' and specialty '{specialtyId}'.");

            // get the subjects from CourseSubjectSpecialty
            var subjects = cssList.Select(css => css.Subject).ToList();
            
            return _mapper.Map<List<SubjectModel>>(subjects);
        }
        #endregion

        #region Get All CourseSubjectSpecialties
        public async Task<List<CourseSubjectSpecialtyModel>> GetAllCourseSubjectSpecialtiesAsync()
        {
            var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.Course,
                css => css.Subject,
                css => css.Specialty,
                css => css.CreatedByUser
            );

            return _mapper.Map<List<CourseSubjectSpecialtyModel>>(courseSubjectSpecialties);
        }
        #endregion

    }
}
