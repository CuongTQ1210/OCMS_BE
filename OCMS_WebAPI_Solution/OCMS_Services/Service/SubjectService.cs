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

        #region Get Trainees by SubjectId
        public async Task<List<TraineViewModel>> GetTraineesBySubjectIdAsync(string subjectId)
        {
            // Fetch ClassSubjects for the subject
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                cs => cs.SubjectSpecialty.SubjectId == subjectId, // FIXED
                cs => cs.traineeAssigns,
                cs => cs.Class
            );

            if (!classSubjects.Any())
                throw new ArgumentException($"No class-subject relationships found for Subject ID '{subjectId}'.");

            // Collect trainee IDs from all ClassSubjects
            var traineeIds = classSubjects.SelectMany(cs => cs.traineeAssigns.Select(t => t.TraineeId)).Distinct().ToList();
            var users = await _unitOfWork.UserRepository.GetAllAsync(u => traineeIds.Contains(u.UserId));

            // Map to TraineViewModel
            var traineeViewModels = new List<TraineViewModel>();
            foreach (var classSubject in classSubjects)
            {
                var trainees = classSubject.traineeAssigns.Select(t => new TraineViewModel
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
        #endregion

        #region Get All Subjects
        public async Task<IEnumerable<GetAllSubjectModel>> GetAllSubjectsAsync()
        {
            var subjects = await _unitOfWork.SubjectRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GetAllSubjectModel>>(subjects);
        }
        #endregion

        #region Get Subject by Id
        public async Task<SubjectModel> GetSubjectByIdAsync(string subjectId)
        {
            // Get the subject
            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);

            if (subject == null)
                throw new KeyNotFoundException("Subject not found.");

            // Get ClassSubjects for this subject
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                cs => cs.SubjectSpecialty.SubjectId == subjectId,
                cs => cs.Class
            );

            // Get SubjectSpecialties for this subject
            var subjectSpecialties = await _unitOfWork.SubjectSpecialtyRepository.GetAllAsync(
                ss => ss.SubjectId == subjectId,
                ss => ss.Specialty
            );

            // Map to SubjectModel (you may need to adjust your mapping profile)
            var subjectModel = _mapper.Map<SubjectModel>(subject);
            
            // Add related data if needed (depends on your SubjectModel structure)
            // Example: subjectModel.Classes = _mapper.Map<List<ClassModel>>(classSubjects.Select(cs => cs.Class));
            // Example: subjectModel.Specialties = _mapper.Map<List<SpecialtyModel>>(subjectSpecialties.Select(ss => ss.Specialty));

            return subjectModel;
        }
        #endregion

        #region Get Subjects by CourseId
        public async Task<List<SubjectModel>> GetSubjectsByCourseIdAsync(string courseId)
        {
            // Get ClassSubjects for this course
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                cs => cs.ClassId == courseId,
                cs => cs.SubjectSpecialty.Subject
            );

            if (!classSubjects.Any())
                throw new KeyNotFoundException("No subjects found for the given course ID.");

            // Extract subjects from ClassSubjects
            var subjects = classSubjects.Select(cs => cs.SubjectSpecialty.Subject).Distinct().ToList(); // FIXED
            return _mapper.Map<List<SubjectModel>>(subjects);
        }
        #endregion

        #region Create Subject
        public async Task<SubjectModel> CreateSubjectAsync(SubjectDTO dto, string createdByUserId)
        {
            // Validate PassingScore (0-10)
            if (dto.PassingScore < 0 || dto.PassingScore > 10)
                throw new ArgumentException("Passing score must be between 0 and 10.");

            if (await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectName == dto.SubjectName))
                throw new ArgumentException($"Subject with name '{dto.SubjectName}' already exists.");

            // Validate createdByUserId
            if (!await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == createdByUserId))
                throw new ArgumentException("The specified User ID does not exist.");

            // Map DTO to Subject entity
            var subject = _mapper.Map<Subject>(dto);
            subject.SubjectId = GenerateSubjectId(dto.SubjectName);
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
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                cs => cs.SubjectSpecialty.SubjectId == subjectId,
                cs => cs.Class
            );

            // Get all course IDs from class subjects
            var courseIds = classSubjects.Select(cs => cs.ClassId).Distinct().ToList();
            
            // Check if any of these courses are approved
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(
                c => courseIds.Contains(c.CourseId)
            );
            
            var approvedCourses = courses.Where(c => c.Status == CourseStatus.Approved).ToList();
            
            if (approvedCourses.Any())
            {
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.Delete,
                    RequestEntityId = subjectId,
                    Description = $"Request to delete subject {subjectId}",
                    Notes = "Awaiting HeadMaster approval due to approved courses"
                };
                await _requestService.CreateRequestAsync(requestDto, approvedCourses.First().CreatedByUserId);
                throw new InvalidOperationException($"Cannot delete subject {subjectId} because it is linked to approved courses. A request has been sent to the HeadMaster.");
            }

            // Delete related ClassSubjects and their schedules
            foreach (var classSubject in classSubjects)
            {
                var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(s => s.ClassSubjectId == classSubject.ClassSubjectId);
                foreach (var schedule in schedules)
                {
                    await _trainingScheduleService.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                }
                await _unitOfWork.ClassSubjectRepository.DeleteAsync(classSubject.ClassSubjectId);
            }

            // Delete subject
            await _unitOfWork.SubjectRepository.DeleteAsync(subjectId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region helper
        private string GenerateSubjectId(string subjectName)
        {
            // Split the name into words
            var words = subjectName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .ToList();

            string initials;

            if (words.Count >= 3)
            {
                // Take the first letter of the first 3 words
                initials = string.Concat(words.Take(3).Select(word => char.ToUpper(word[0])));
            }
            else
            {
                // If less than 3 words, take the first 3 letters of the first word
                string firstWord = words.First();
                initials = new string(firstWord
                    .Where(char.IsLetter) // ignore non-letters
                    .Take(3)
                    .Select(char.ToUpper)
                    .ToArray());
            }
            // Generate a random 3-digit number (you can adjust length if needed)
            Random rnd = new Random();
            int randomNumber = rnd.Next(1, 1000); // Generates number between 1 and 999
            string formattedNumber = randomNumber.ToString("D3");
            return $"{initials}{formattedNumber}";
        }
        #endregion
    }
}