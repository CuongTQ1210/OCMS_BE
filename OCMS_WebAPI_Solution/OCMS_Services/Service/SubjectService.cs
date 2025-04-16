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
    public class SubjectService : ISubjectService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITrainingScheduleService _trainingScheduleService;
        public SubjectService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public SubjectService(UnitOfWork unitOfWork, IMapper mapper, ITrainingScheduleService trainingScheduleService) : this(unitOfWork, mapper)
        {
            _trainingScheduleService = trainingScheduleService;
        }

        public async Task<IEnumerable<SubjectModel>> GetAllSubjectsAsync()
        {
            var subjects = await _unitOfWork.SubjectRepository.GetAllAsync(
                p => p.Instructors,
                p => p.Schedules
                );
            return _mapper.Map<IEnumerable<SubjectModel>>(subjects);
        }

        public async Task<SubjectModel> GetSubjectByIdAsync(string subjectId)
        {
            var subject = await _unitOfWork.SubjectRepository.GetAsync(
                p=> p.SubjectId== subjectId,
                p => p.Instructors,
                p => p.Schedules
                );
            if (subject == null)
                throw new KeyNotFoundException("Subject not found.");

            return _mapper.Map<SubjectModel>(subject);
        }

        public async Task<List<SubjectModel>> GetSubjectsByCourseIdAsync(string courseId)
        {
            var subjects = await _unitOfWork.SubjectRepository.FindAsync(
                s => s.CourseId == courseId
            );

            if (subjects == null || !subjects.Any())
                throw new KeyNotFoundException("No subjects found for the given course ID.");

            return _mapper.Map<List<SubjectModel>>(subjects);
        }

        public async Task<SubjectModel> CreateSubjectAsync(SubjectDTO dto, string createdByUserId)
        {
            // Validate PassingScore (0-10)
            if (dto.PassingScore < 0 || dto.PassingScore > 10)
                throw new ArgumentException("Passing score must be between 0 and 10.");

            // Ensure CourseId exists
            var courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == dto.CourseId);
            if (!courseExists)
                throw new ArgumentException("Course does not exist.");
            var subjectExisted = await _unitOfWork.SubjectRepository.ExistsAsync(c=> c.SubjectId == dto.SubjectId);
            if (subjectExisted)
                throw new ArgumentException("Subject already existed.");
            var subjectExisted2 = await _unitOfWork.SubjectRepository.ExistsAsync(c => c.SubjectName == dto.SubjectName);
            if (subjectExisted2)
                throw new ArgumentException("This Subject name already existed.");
            var userExists = await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == createdByUserId);
            if (!userExists)
            {
                throw new Exception("The specified User ID does not exist.");
            }
            var subject = _mapper.Map<Subject>(dto);
            subject.SubjectId = dto.SubjectId;
            subject.CreateByUserId = createdByUserId;
            
            subject.CreatedAt = DateTime.Now;
            subject.UpdatedAt = DateTime.Now;

            await _unitOfWork.SubjectRepository.AddAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubjectModel>(subject);
        }

        public async Task<SubjectModel> UpdateSubjectAsync(string subjectId, SubjectDTO dto)
        {
            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);
            if (subject == null)
                throw new KeyNotFoundException("Subject not found.");

            // Validate PassingScore (0-10)
            if (dto.PassingScore < 0 || dto.PassingScore > 10)
                throw new ArgumentException("Passing score must be between 0 and 10.");

            // Ensure CourseId exists
            var courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == dto.CourseId);
            if (!courseExists)
                throw new ArgumentException("Course does not exist.");
            var subjectExisted2 = await _unitOfWork.SubjectRepository.ExistsAsync(c => c.SubjectName == dto.SubjectName);
            if (subjectExisted2)
                throw new ArgumentException("This Subject name already existed.");
            _mapper.Map(dto, subject);
            subject.UpdatedAt = DateTime.Now;
            _unitOfWork.SubjectRepository.UpdateAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubjectModel>(subject);
        }

        public async Task<bool> DeleteSubjectAsync(string subjectId)
        {
            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);
            if (subject == null)
                throw new KeyNotFoundException("Subject not found.");

            // Get all training schedules linked to this subject
            var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(s => s.SubjectID == subjectId);

            foreach (var schedule in schedules)
            {
                await _trainingScheduleService.DeleteTrainingScheduleAsync(schedule.ScheduleID); // Ensure schedules and assignments are deleted
            }

            _unitOfWork.SubjectRepository.DeleteAsync(subjectId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
