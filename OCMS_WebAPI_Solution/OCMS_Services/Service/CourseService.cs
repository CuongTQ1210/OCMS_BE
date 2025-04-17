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
            if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved || trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Rejected)
                throw new Exception("Training Plan ID already approved or rejected!");
            var level = trainingPlan.PlanLevel.ToString();
            var course = _mapper.Map<Course>(dto);
            course.CourseId = dto.CourseId;
            course.CourseName = dto.CourseName;
            course.TrainingPlanId= dto.TrainingPlanId;
            course.TrainingPlan = trainingPlan;
            course.CreatedByUserId = createdByUserId;
            course.CreatedAt = DateTime.Now;
            course.UpdatedAt = DateTime.Now;
            course.Status = CourseStatus.Pending;
            course.Progress = Progress.NotYet;
            course.CourseLevel = (CourseLevel)trainingPlan.PlanLevel;
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

        public async Task<CourseModel> UpdateCourseAsync(string id, CourseUpdateDTO dto, string updatedByUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null) throw new Exception("Course Id does not exist!!"); ;
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(dto.TrainingPlanId);
            if (trainingPlan == null)
                throw new Exception("Training Plan ID does not exist. Please provide a valid Training Plan.");
            if (course.Status == CourseStatus.Approved)
                throw new Exception("Course already approved! Please send request to update");
            _mapper.Map(dto, course);
            course.TrainingPlanId = dto.TrainingPlanId;
            course.CourseLevel = dto.CourseLevel;
            course.UpdatedAt = DateTime.Now;

            _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseModel>(course);
        }
    }
}
