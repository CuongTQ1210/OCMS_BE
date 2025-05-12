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
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

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

    #region Create Course
    public async Task<CourseModel> CreateCourseAsync(CourseDTO dto, string createdByUserId)
    {
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

        var courseId = await GenerateCourseId(dto.CourseName, dto.CourseLevel, dto.CourseRelatedId);
        var existedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
        if(existedCourse!= null)
        {
            throw new ArgumentException($"Course with this level {dto.CourseLevel} already existed for this course {dto.CourseRelatedId}");
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
    #endregion

    #region Get All Courses
    public async Task<IEnumerable<CourseModel>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllWithIncludesAsync(query =>
            query.Include(c => c.CourseSubjectSpecialties)
                 .ThenInclude(css => css.Subject)
                 .Include(c => c.CourseSubjectSpecialties)
                 .ThenInclude(css => css.Trainees)
                 .Include(c => c.CourseSubjectSpecialties)
                 .ThenInclude(css => css.Instructors)
                 .Include(c => c.CourseSubjectSpecialties)
                 .ThenInclude(css => css.Schedules)
                 .Include(c => c.CourseSubjectSpecialties)
                 .ThenInclude(css => css.Specialty)
        );

        return _mapper.Map<IEnumerable<CourseModel>>(courses);
    }
    #endregion

    #region Get Course by ID
    public async Task<CourseModel?> GetCourseByIdAsync(string id)
    {
        var course = await _courseRepository.GetWithIncludesAsync(
            c => c.CourseId == id,
            query => query.Include(c => c.CourseSubjectSpecialties)
                          .ThenInclude(css => css.Subject)
                          .Include(c => c.CourseSubjectSpecialties)
                          .ThenInclude(css => css.Trainees)
                          .Include(c => c.CourseSubjectSpecialties)
                          .ThenInclude(css => css.Instructors)
                          .Include(c => c.CourseSubjectSpecialties)
                          .ThenInclude(css => css.Schedules)
                          .Include(c => c.CourseSubjectSpecialties)
                          .ThenInclude(css => css.Specialty)
        );

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
    #region
    public async Task<bool> SendCourseRequestForApprove(string courseId, string createdByUserId)
    {
        var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            throw new KeyNotFoundException($"Course with id {courseId} does not exist!");
        }

        var requestDto = new RequestDTO
        {
            RequestEntityId = courseId,
            RequestType = RequestType.NewCourse,
            Description = $"Request to approve new course '{course.CourseName}'",
            Notes = null 
        };

        await _requestService.CreateRequestAsync(requestDto, createdByUserId);
        return true;
    }

    #endregion

    #region Helper Method
    public async Task<string> GenerateCourseId(string courseName, string level, string? relatedCourseId = null)
    {
        if (level == "Initial")
        {
            // Use your original initials extraction logic
            string courseInitials = string.Concat(courseName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Take(3)
                .Select(word => char.ToUpper(word[0])));

            // Always start Initial with 101
            return $"{courseInitials}101";
        }

        if (string.IsNullOrEmpty(relatedCourseId))
            throw new ArgumentException("Related course ID is required for non-initial levels.");

        var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(relatedCourseId);
        if (relatedCourse == null)
            throw new ArgumentException("The related course does not exist.");

        // Extract initials (alphabetic part)
        string initialsPart = new(relatedCourse.CourseId.TakeWhile(char.IsLetter).ToArray());

        // Extract number (numeric part)
        string numberPart = new(relatedCourse.CourseId.SkipWhile(char.IsLetter).ToArray());

        if (!int.TryParse(numberPart, out int baseCode))
            throw new InvalidOperationException("Related course ID does not contain a valid numeric part.");

        // Determine new code based on level
        int newCode = level switch
        {
            "Professional" => baseCode + 100,
            "Recurrent" => baseCode + 10,
            "Relearn" => baseCode + 1,
            _ => throw new InvalidOperationException("Invalid CourseLevel.")
        };

        return $"{initialsPart}{newCode}";
    }
    #endregion
}