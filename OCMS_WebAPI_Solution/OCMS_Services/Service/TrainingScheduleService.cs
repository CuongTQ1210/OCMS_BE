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
    public class TrainingScheduleService : ITrainingScheduleService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrainingScheduleService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Retrieves all training schedules with related Subject, Instructor, and CreatedByUser data.
        /// </summary>
        public async Task<IEnumerable<TrainingScheduleModel>> GetAllTrainingSchedulesAsync()
        {
            var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(
                s => s.Subject,
                s => s.Instructor,
                s => s.CreatedByUser
            );
            return _mapper.Map<IEnumerable<TrainingScheduleModel>>(schedules);
        }

        /// <summary>
        /// Retrieves a training schedule by its ID, including related data.
        /// </summary>
        public async Task<TrainingScheduleModel> GetTrainingScheduleByIdAsync(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.Subject,
                s => s.Instructor,
                s => s.CreatedByUser
            );
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");

            return _mapper.Map<TrainingScheduleModel>(schedule);
        }

        /// <summary>
        /// Creates a new training schedule with the provided DTO and user ID.
        /// </summary>
        public async Task<TrainingScheduleModel> CreateTrainingScheduleAsync(TrainingScheduleDTO dto, string createdByUserId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrEmpty(createdByUserId))
                throw new ArgumentException("CreatedBy user ID cannot be null or empty.", nameof(createdByUserId));

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectID);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectID} does not exist.");

            // Validate CreatedBy user
            var userExists = await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == createdByUserId);
            if (!userExists)
                throw new ArgumentException($"User with ID {createdByUserId} does not exist.");

            // Generate ScheduleID in the format SCD-XXXXXX
            string scheduleId;
            do
            {
                scheduleId = GenerateScheduleId();
            } while (await _unitOfWork.TrainingScheduleRepository.ExistsAsync(s => s.ScheduleID == scheduleId));

            // Map DTO to entity
            var schedule = _mapper.Map<TrainingSchedule>(dto);
            schedule.ScheduleID = scheduleId; // Set custom ID
            schedule.CreatedBy = createdByUserId;
            schedule.CreatedDate = DateTime.UtcNow;
            schedule.ModifiedDate = DateTime.UtcNow;

            // Set default values for fields not in DTO (assuming they are required)
            schedule.StartDateTime = DateTime.UtcNow; // Placeholder; adjust as needed
            schedule.EndDateTime = DateTime.UtcNow.AddHours(1); // Placeholder; adjust as needed

            await _unitOfWork.TrainingScheduleRepository.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Fetch with related data
            var createdSchedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == schedule.ScheduleID,
                s => s.Subject,
                s => s.Instructor,
                s => s.CreatedByUser
            );
            return _mapper.Map<TrainingScheduleModel>(createdSchedule);
        }

        /// <summary>
        /// Updates an existing training schedule with the provided DTO.
        /// </summary>
        public async Task<TrainingScheduleModel> UpdateTrainingScheduleAsync(string scheduleId, TrainingScheduleDTO dto)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectID);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectID} does not exist.");

            // Map DTO to existing entity, preserving fields not in DTO
            _mapper.Map(dto, schedule);
            schedule.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Fetch with related data
            var updatedSchedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.Subject,
                s => s.Instructor,
                s => s.CreatedByUser
            );
            return _mapper.Map<TrainingScheduleModel>(updatedSchedule);
        }

        /// <summary>
        /// Deletes a training schedule by its ID.
        /// </summary>
        public async Task<bool> DeleteTrainingScheduleAsync(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");

            _unitOfWork.TrainingScheduleRepository.DeleteAsync(scheduleId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Generates a ScheduleID in the format SCD-XXXXXX where XXXXXX is a random 6-digit number.
        /// </summary>
        private string GenerateScheduleId()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 1000000); // Generates a number between 0 and 999999
            return $"SCD-{randomNumber:000000}"; // Ensures 6 digits with leading zeros
        }
    }
}
