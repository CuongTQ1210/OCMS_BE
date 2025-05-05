using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using PuppeteerSharp.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class SubjectService : ISubjectService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITrainingScheduleService _trainingScheduleService;
        private readonly ICourseRepository _courseRepository;
        private readonly IRequestService _requestService;

        public SubjectService(UnitOfWork unitOfWork, IMapper mapper, ITrainingScheduleService trainingScheduleService,
            ICourseRepository courseRepository, IRequestService requestService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
        }

        public async Task<List<TraineViewModel>> GetTraineesBySubjectIdAsync(string subjectId)
        {
            // Fetch CourseSubjectSpecialties for the subject
            var cssList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.SubjectId == subjectId,
                css => css.Trainees,
                css => css.Course
            );
            if (!cssList.Any())
                throw new ArgumentException($"No course-subject relationships found for Subject ID '{subjectId}'.");

            // Collect trainee IDs from all CourseSubjectSpecialties
            var traineeIds = cssList.SelectMany(css => css.Trainees.Select(t => t.TraineeId)).Distinct().ToList();
            var users = await _unitOfWork.UserRepository.GetAllAsync(u => traineeIds.Contains(u.UserId));

            // Map to TraineViewModel
            var traineeViewModels = new List<TraineViewModel>();
            foreach (var css in cssList)
            {
                var trainees = css.Trainees.Select(t => new TraineViewModel
                {
                    TraineeId = t.TraineeId,
                    Name = users.FirstOrDefault(u => u.UserId == t.TraineeId)?.FullName,
                    Email = users.FirstOrDefault(u => u.UserId == t.TraineeId)?.Email,
                    TraineeAssignId = t.TraineeAssignId
                }).ToList();
                traineeViewModels.AddRange(trainees);
            }

            return traineeViewModels.Any() ? traineeViewModels : new List<TraineViewModel>();
        }

        #region Get All Subjects
        public async Task<IEnumerable<SubjectModel>> GetAllSubjectsAsync()
        {
            var subjects = await _unitOfWork.SubjectRepository.GetAllAsync(
                s => s.CourseSubjectSpecialties,
                s => s.CourseSubjectSpecialties.Select(css => css.Course),
                s => s.CourseSubjectSpecialties.Select(css => css.Specialty)
            );

            var subjectModels = _mapper.Map<IEnumerable<SubjectModel>>(subjects);
            return subjectModels;
        }
        #endregion

        #region Get Subject by Id
        public async Task<SubjectModel> GetSubjectByIdAsync(string subjectId)
        {
            var subject = await _unitOfWork.SubjectRepository.GetAsync(
                s => s.SubjectId == subjectId,
                s => s.CourseSubjectSpecialties,
                s => s.CourseSubjectSpecialties.Select(css => css.Course),
                s => s.CourseSubjectSpecialties.Select(css => css.Specialty)
            );
            if (subject == null)
                throw new KeyNotFoundException("Subject not found.");

            return _mapper.Map<SubjectModel>(subject);
        }
        #endregion

        #region Get Subjects by CourseId
        public async Task<List<SubjectModel>> GetSubjectsByCourseIdAsync(string courseId)
        {
            var cssList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.CourseId == courseId,
                css => css.Subject,
                css => css.Specialty
            );

            if (!cssList.Any())
                throw new KeyNotFoundException("No subjects found for the given course ID.");

            var subjects = cssList.Select(css => css.Subject).ToList();
            return _mapper.Map<List<SubjectModel>>(subjects);
        }
        #endregion

        #region Create Subject
        public async Task<SubjectModel> CreateSubjectAsync(SubjectDTO dto, string createdByUserId)
        {
            // Validate PassingScore (0-10)
            if (dto.PassingScore < 0 || dto.PassingScore > 10)
                throw new ArgumentException("Passing score must be between 0 and 10.");

            // Validate SubjectId and SubjectName uniqueness
            if (await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectId))
                throw new ArgumentException($"Subject with ID '{dto.SubjectId}' already exists.");
            if (await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectName == dto.SubjectName))
                throw new ArgumentException($"Subject with name '{dto.SubjectName}' already exists.");

            // Validate createdByUserId
            if (!await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == createdByUserId))
                throw new ArgumentException("The specified User ID does not exist.");

            // Map DTO to Subject entity
            var subject = _mapper.Map<Subject>(dto);
            subject.CreateByUserId = createdByUserId;
            subject.CreatedAt = DateTime.Now;
            subject.UpdatedAt = DateTime.Now;

            // Add subject to repository
            await _unitOfWork.SubjectRepository.AddAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubjectModel>(subject);
        }
        #endregion

        #region Update Subject
        public async Task<SubjectModel> UpdateSubjectAsync(string subjectId, SubjectDTO dto)
        {
            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);
            if (subject == null)
                throw new KeyNotFoundException("Subject not found.");

            // Validate PassingScore (0-10)
            if (dto.PassingScore < 0 || dto.PassingScore > 10)
                throw new ArgumentException("Passing score must be between 0 and 10.");

            // Check if subject name is changed and unique
            if (dto.SubjectName != subject.SubjectName &&
                await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectName == dto.SubjectName))
                throw new ArgumentException($"Subject with name '{dto.SubjectName}' already exists.");

            // Map DTO to subject entity
            _mapper.Map(dto, subject);
            subject.UpdatedAt = DateTime.Now;

            // Update subject in repository
            await _unitOfWork.SubjectRepository.UpdateAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubjectModel>(subject);
        }
        #endregion

        #region Delete Subject
        public async Task<bool> DeleteSubjectAsync(string subjectId)
        {
            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);
            if (subject == null)
                throw new KeyNotFoundException("Subject not found.");

            // Check if subject is linked to any approved courses
            var cssList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.SubjectId == subjectId,
                css => css.Course
            );
            var approvedCourses = cssList.Where(css => css.Course.Status == CourseStatus.Approved).ToList();
            if (approvedCourses.Any())
            {
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.Delete,
                    RequestEntityId = subjectId,
                    Description = $"Request to delete subject {subjectId}",
                    Notes = "Awaiting HeadMaster approval due to approved courses"
                };
                await _requestService.CreateRequestAsync(requestDto, approvedCourses.First().Course.CreatedByUserId);
                throw new InvalidOperationException($"Cannot delete subject {subjectId} because it is linked to approved courses. A request has been sent to the HeadMaster.");
            }

            // Delete related CourseSubjectSpecialties and their schedules
            foreach (var css in cssList)
            {
                var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(s => s.CourseSubjectSpecialtyId == css.Id);
                foreach (var schedule in schedules)
                {
                    await _trainingScheduleService.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                }
                await _unitOfWork.CourseSubjectSpecialtyRepository.DeleteAsync(css.Id);
            }

            // Delete subject
            await _unitOfWork.SubjectRepository.DeleteAsync(subjectId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion
    }
}
