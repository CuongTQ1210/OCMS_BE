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
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class CourseService : ICourseService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        public CourseService(UnitOfWork unitOfWork, IMapper mapper, ICourseRepository courseRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _courseRepository = courseRepository;

        }

        #region Create Course
        public async Task<CourseModel> CreateCourseAsync(CourseDTO dto, string createdByUserId)
        {
            // Check if Training Plan exists
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(dto.TrainingPlanId);
            if (trainingPlan == null)
                throw new Exception("Training Plan ID does not exist. Please provide a valid Training Plan.");

            // Validate Training Plan status
            if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved ||
                trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Rejected)
                throw new Exception("Training Plan ID already approved or rejected!");
            // Convert empty CourseRelatedId to null
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
            {
                dto.CourseRelatedId = null;
            }
            // Set course level to match training plan level
            var courseLevel = (CourseLevel)trainingPlan.PlanLevel;

            // Validate CourseRelatedId based on course level and ensure it exists if provided
            if (courseLevel == CourseLevel.Recurrent || courseLevel == CourseLevel.Relearn)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new Exception("CourseRelatedId is required for Recurrent or Relearn course levels.");

                // Validate that the related course exists
                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new Exception("The specified CourseRelatedId does not exist.");
            }
            else if (courseLevel == CourseLevel.Initial)
            {
                if (!string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new Exception("CourseRelatedId must be null for Initial course level.");
            }

            // Map DTO to Course entity
            var course = _mapper.Map<Course>(dto);
            course.CourseId = dto.CourseId;
            course.CourseName = dto.CourseName;
            course.TrainingPlanId = dto.TrainingPlanId;
            course.TrainingPlan = trainingPlan;
            course.CreatedByUserId = createdByUserId;
            course.CreatedAt = DateTime.Now;
            course.UpdatedAt = DateTime.Now;
            course.Status = CourseStatus.Pending;
            course.Progress = Progress.NotYet;
            course.CourseLevel = courseLevel;

            // Add course to repository and save
            await _unitOfWork.CourseRepository.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseModel>(course);
        }
        #endregion

        #region Get all Courses
        public async Task<IEnumerable<CourseModel>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(
                p => p.Subjects,
                p => p.Trainees
                );
            return _mapper.Map<IEnumerable<CourseModel>>(courses);
        }
        #endregion

        #region Get Course by ID
        public async Task<CourseModel?> GetCourseByIdAsync(string id)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(id);
            return _mapper.Map<CourseModel>(course);
        }
        #endregion

        #region Delete Course
        public async Task<bool> DeleteCourseAsync(string id)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null)
            {
                throw new Exception("Course does not exist.");
            }
            if (course.Status == CourseStatus.Approved )
                throw new Exception("Course already approved! Please send request to delete");
            await _unitOfWork.CourseRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Update Course
        public async Task<CourseModel> UpdateCourseAsync(string id, CourseUpdateDTO dto, string updatedByUserId)
        {
            // Validate course existence
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null)
                throw new Exception("Course Id does not exist!!");

            // Check if course is approved
            if (course.Status == CourseStatus.Approved)
                throw new Exception("Course already approved! Please send request to update");
            // Convert empty CourseRelatedId to null
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
            {
                dto.CourseRelatedId = null;
            }
            // Validate CourseRelatedId based on course level and ensure it exists if provided
            if (course.CourseLevel == CourseLevel.Recurrent || course.CourseLevel == CourseLevel.Relearn)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new Exception("CourseRelatedId is required for Recurrent or Relearn course levels.");

                // Validate that the related course exists
                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new Exception("The specified CourseRelatedId does not exist.");
            }
            else if (course.CourseLevel == CourseLevel.Initial)
            {
                if (!string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new Exception("CourseRelatedId must be null for Initial course level.");
            }
            // Map DTO to course entity
            _mapper.Map(dto, course);
            course.UpdatedAt = DateTime.Now;

            // Update course in repository and save
            await _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseModel>(course);
        }
        #endregion
    }
}
