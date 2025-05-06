using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories.IRepository;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public class CourseService : ICourseService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICourseRepository _courseRepository;
    private readonly IRequestService _requestService; // Added for request creation

    public CourseService(UnitOfWork unitOfWork, IMapper mapper, ICourseRepository courseRepository, IRequestService requestService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _courseRepository = courseRepository;
        _requestService = requestService;
    }

    public async Task<CourseModel> CreateCourseAsync(CourseDTO dto, string createdByUserId)
    {
        // Check if Training Plan exists
        var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(dto.TrainingPlanId);
        if (trainingPlan == null)
            throw new Exception("Training Plan ID does not exist. Please provide a valid Training Plan.");

        // Validate Training Plan status
        if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved ||
            trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Rejected)
            throw new Exception("Training Plan is already approved or rejected!");

        // Convert empty CourseRelatedId to null
        if (string.IsNullOrEmpty(dto.CourseRelatedId))
        {
            dto.CourseRelatedId = null;
        }

        // Parse CourseLevel from DTO
        if (!Enum.TryParse<CourseLevel>(dto.CourseLevel, true, out var courseLevel))
            throw new ArgumentException("Invalid CourseLevel provided.");

        // Validate CourseRelatedId based on CourseLevel
        if (courseLevel == CourseLevel.Initial)
        {
            if (!string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId must be null for Initial course level.");
        }
        else if (courseLevel == CourseLevel.Professional)
        {
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId is required for Professional course level.");

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
            if (relatedCourse == null)
                throw new ArgumentException("The specified CourseRelatedId does not exist.");
            if (relatedCourse.CourseLevel != CourseLevel.Initial)
                throw new ArgumentException("The related course for a Professional course must have Initial level.");
        }
        else if (courseLevel == CourseLevel.Recurrent)
        {
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId is required for Recurrent course level.");

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
            if (relatedCourse == null)
                throw new ArgumentException("The specified CourseRelatedId does not exist.");
            if (relatedCourse.CourseLevel != CourseLevel.Initial && relatedCourse.CourseLevel != CourseLevel.Professional)
                throw new ArgumentException("The related course for a Recurrent course must have Initial or Professional level.");
        }
        else if (courseLevel == CourseLevel.Relearn)
        {
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId is required for Relearn course level.");

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
            if (relatedCourse == null)
                throw new ArgumentException("The specified CourseRelatedId does not exist.");
            if (relatedCourse.CourseLevel != CourseLevel.Initial &&
                relatedCourse.CourseLevel != CourseLevel.Professional &&
                relatedCourse.CourseLevel != CourseLevel.Recurrent)
                throw new ArgumentException("The related course for a Relearn course must have Initial, Professional, or Recurrent level.");
        }
        else
        {
            throw new ArgumentException("Unsupported CourseLevel provided.");
        }

        // Generate CourseId
        string courseId;
        if (courseLevel == CourseLevel.Initial)
        {
            courseId = string.IsNullOrEmpty(dto.CourseId) ? Guid.NewGuid().ToString() : dto.CourseId;
            // Validate uniqueness of CourseId
            var existingCourse = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (existingCourse != null)
                throw new ArgumentException($"CourseId {courseId} already exists.");
        }
        else
        {
            // For Professional, Recurrent, Relearn, use CourseRelatedId with suffix
            courseId = courseLevel switch
            {
                CourseLevel.Professional => $"{dto.CourseRelatedId}-PRO",
                CourseLevel.Recurrent => $"{dto.CourseRelatedId}-REC",
                CourseLevel.Relearn => $"{dto.CourseRelatedId}-REL",
                _ => throw new InvalidOperationException("Invalid CourseLevel for CourseId generation.")
            };

            // Validate uniqueness of generated CourseId
            var existingCourse = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (existingCourse != null)
                throw new ArgumentException($"Generated CourseId {courseId} already exists.");
        }

        // Map DTO to Course entity
        var course = _mapper.Map<Course>(dto);
        course.CourseId = courseId;
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

    #region Get All Courses
    public async Task<IEnumerable<CourseModel>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllWithDetailsAsync();
        return _mapper.Map<IEnumerable<CourseModel>>(courses);
    }
    #endregion

    #region Get Course by ID
    public async Task<CourseModel?> GetCourseByIdAsync(string id)
    {
        var course = await _courseRepository.GetByIdWithDetailsAsync(id);
        return course == null ? null : _mapper.Map<CourseModel>(course);
    }
    #endregion

    #region Delete Course
    public async Task<bool> DeleteCourseAsync(string id)
    {
        var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
        if (course == null)
            throw new Exception("Course does not exist.");

        if (course.Status == CourseStatus.Approved)
        {
            // Create a request for deletion
            var requestDto = new RequestDTO
            {
                RequestType = RequestType.Delete, // Updated to match CreateCourseAsync
                RequestEntityId = id,
                Description = $"Request to delete course {id}",
                Notes = "Awaiting HeadMaster approval"
            };
            await _requestService.CreateRequestAsync(requestDto, course.CreatedByUserId);
            throw new InvalidOperationException($"Cannot delete course {id} because it is Approved. A request has been sent to the HeadMaster for approval.");
        }

        // Allow deletion only for Pending or Draft statuses
        if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected)
        {
            await _unitOfWork.CourseRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        throw new InvalidOperationException($"Cannot delete course {id} because its status is {course.Status}. Only Pending or Draft courses can be deleted.");
    }
    #endregion

    #region Update Course
    public async Task<CourseModel> UpdateCourseAsync(string id, CourseUpdateDTO dto, string updatedByUserId)
    {
        // Validate course existence
        var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
        if (course == null)
            throw new Exception("Course Id does not exist!");

        // Check if course is approved
        if (course.Status == CourseStatus.Approved)
        {
            // Create a request for update
            var proposedChanges = JsonSerializer.Serialize(dto);
            var requestDto = new RequestDTO
            {
                RequestType = RequestType.Update, // Updated to match CreateCourseAsync
                RequestEntityId = id,
                Description = $"Request to update course {id}",
                Notes = $"Proposed changes: {proposedChanges}"
            };
            await _requestService.CreateRequestAsync(requestDto, updatedByUserId);
            return _mapper.Map<CourseModel>(course); // Return unchanged course
        }

        // Convert empty CourseRelatedId to null
        if (string.IsNullOrEmpty(dto.CourseRelatedId))
        {
            dto.CourseRelatedId = null;
        }

        // Validate CourseRelatedId based on CourseLevel
        if (course.CourseLevel == CourseLevel.Initial)
        {
            if (!string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId must be null for Initial course level.");
        }
        else if (course.CourseLevel == CourseLevel.Professional)
        {
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId is required for Professional course level.");

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
            if (relatedCourse == null)
                throw new ArgumentException("The specified CourseRelatedId does not exist.");
            if (relatedCourse.CourseLevel != CourseLevel.Initial)
                throw new ArgumentException("The related course for a Professional course must have Initial level.");
        }
        else if (course.CourseLevel == CourseLevel.Recurrent)
        {
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId is required for Recurrent course level.");

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
            if (relatedCourse == null)
                throw new ArgumentException("The specified CourseRelatedId does not exist.");
            if (relatedCourse.CourseLevel != CourseLevel.Initial && relatedCourse.CourseLevel != CourseLevel.Professional)
                throw new ArgumentException("The related course for a Recurrent course must have Initial or Professional level.");
        }
        else if (course.CourseLevel == CourseLevel.Relearn)
        {
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
                throw new ArgumentException("CourseRelatedId is required for Relearn course level.");

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
            if (relatedCourse == null)
                throw new ArgumentException("The specified CourseRelatedId does not exist.");
            if (relatedCourse.CourseLevel != CourseLevel.Initial &&
                relatedCourse.CourseLevel != CourseLevel.Professional &&
                relatedCourse.CourseLevel != CourseLevel.Recurrent)
                throw new ArgumentException("The related course for a Relearn course must have Initial, Professional, or Recurrent level.");
        }
        else
        {
            throw new ArgumentException("Unsupported CourseLevel in course.");
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