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

        public async Task<CourseModel> CreateCourseAsync(CourseDTO dto, string createdByUserId)
        {

            
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(dto.TrainingPlanId);
            if (trainingPlan == null)
                throw new Exception("Training Plan ID does not exist. Please provide a valid Training Plan.");
            if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved && trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Rejected)
                throw new Exception("Training Plan ID already approved or rejected!");
            var course = _mapper.Map<Course>(dto);
            course.CourseId = dto.CourseId;
            course.CourseName = dto.CourseName;
            course.TrainingPlanId= dto.TrainingPlanId;
            course.CourseLevel = dto.CourseLevel;
            course.TrainingPlan = trainingPlan;
            course.CreatedByUserId = createdByUserId;
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.Status = CourseStatus.Pending;
            course.Progress = Progress.NotYet;
            await _unitOfWork.CourseRepository.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseModel>(course);
        }

        public async Task<IEnumerable<CourseModel>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(
                p => p.Subjects,
                p => p.Trainees
                );
            return _mapper.Map<IEnumerable<CourseModel>>(courses);
        }

        public async Task<CourseModel?> GetCourseByIdAsync(string id)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(id);
            return _mapper.Map<CourseModel>(course);
        }

        public async Task<bool> DeleteCourseAsync(string id)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null) return false;

            _unitOfWork.CourseRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<CourseModel> UpdateCourseAsync(string id, CourseUpdateDTO dto, string updatedByUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null) throw new Exception("Course Id does not exist!!"); ;
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(dto.TrainingPlanId);
            if (trainingPlan == null)
                throw new Exception("Training Plan ID does not exist. Please provide a valid Training Plan.");
            _mapper.Map(dto, course);
            course.TrainingPlanId = dto.TrainingPlanId;
            course.CourseLevel = dto.CourseLevel;
            course.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseModel>(course);
        }
    }
}
