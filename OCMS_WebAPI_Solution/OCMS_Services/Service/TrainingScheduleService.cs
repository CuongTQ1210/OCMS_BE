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
using System.Text.Json;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class TrainingScheduleService : ITrainingScheduleService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IInstructorAssignmentService _instructorAssignmentService;
        private readonly IRequestService _requestService;
        private readonly Lazy<ITrainingScheduleService> _trainingScheduleService;
        private readonly Lazy<ITrainingPlanService> _trainingPlanService;
        public TrainingScheduleService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            IInstructorAssignmentService instructorAssignmentService,
            IRequestService requestService,
            Lazy<ITrainingScheduleService> trainingScheduleService,
            Lazy<ITrainingPlanService> trainingPlanService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _instructorAssignmentService = instructorAssignmentService ?? throw new ArgumentNullException(nameof(instructorAssignmentService));
            _requestService = requestService;
            _trainingPlanService = trainingPlanService ?? throw new ArgumentNullException(nameof(trainingPlanService));
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
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
        /// Creates a new training schedule and associated instructor assignment.
        /// </summary>
        public async Task<TrainingScheduleModel> CreateTrainingScheduleAsync(TrainingScheduleDTO dto, string createdByUserId)
        {
            if (string.IsNullOrEmpty(createdByUserId))
                throw new ArgumentException("CreatedBy user ID cannot be null or empty.", nameof(createdByUserId));

            var userExists = await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == createdByUserId);
            if (!userExists)
                throw new ArgumentException($"User with ID {createdByUserId} does not exist.");

            await ValidateTrainingScheduleAsync(dto);

            // Generate unique ScheduleID
            string scheduleId;
            do
            {
                scheduleId = GenerateScheduleId();
            } while (await _unitOfWork.TrainingScheduleRepository.ExistsAsync(s => s.ScheduleID == scheduleId));

            // Create or update InstructorAssignment
            await ManageInstructorAssignment(dto.SubjectID, dto.InstructorID, createdByUserId);

            // Map DTO to entity
            var schedule = _mapper.Map<TrainingSchedule>(dto);
            schedule.ScheduleID = scheduleId;
            schedule.CreatedBy = createdByUserId;
            schedule.CreatedDate = DateTime.UtcNow;
            schedule.ModifiedDate = DateTime.UtcNow;
            schedule.Status = ScheduleStatus.Pending;
            schedule.StartDateTime = dto.StartDay;
            schedule.EndDateTime = dto.EndDay;
            await _unitOfWork.TrainingScheduleRepository.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            var createdSchedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == schedule.ScheduleID,
                s => s.Subject,
                s => s.Instructor,
                s => s.CreatedByUser
            );

            return _mapper.Map<TrainingScheduleModel>(createdSchedule);
        }

        #region Update Training Schedule
        public async Task<TrainingScheduleModel> UpdateTrainingScheduleAsync(string scheduleId, TrainingScheduleDTO dto)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");

            await ValidateTrainingScheduleAsync(dto, scheduleId);

            // Apply update
            _mapper.Map(dto, schedule);
            schedule.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Update InstructorAssignment if needed
            await ManageInstructorAssignment(dto.SubjectID, dto.InstructorID, schedule.CreatedBy);

            var updatedSchedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.Subject,
                s => s.Instructor,
                s => s.CreatedByUser
            );

            return _mapper.Map<TrainingScheduleModel>(updatedSchedule);
        }


        #endregion

        // Other methods (GetAll, GetById, Delete) remain unchanged...

        /// <summary>
        /// Manages the instructor assignment (create or update) based on subject and instructor.
        /// </summary>
        public async Task ManageInstructorAssignment(string subjectId, string instructorId, string assignByUserId)
        {
            var existingAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.SubjectId == subjectId
            );

            if (existingAssignment == null)
            {
                var assignmentDto = new InstructorAssignmentDTO
                {
                    SubjectId = subjectId,
                    InstructorId = instructorId,
                    Notes = "Automatically created from training schedule"
                };
                await _instructorAssignmentService.CreateInstructorAssignmentAsync(assignmentDto, assignByUserId);
            }
            else if (existingAssignment.InstructorId != instructorId)
            {
                var assignmentDto = new InstructorAssignmentDTO
                {
                    SubjectId = subjectId,
                    InstructorId = instructorId,
                    Notes = existingAssignment.Notes ?? "Updated from training schedule"
                };
                await _instructorAssignmentService.UpdateInstructorAssignmentAsync(existingAssignment.AssignmentId, assignmentDto);
            }
        }

        /// <summary>
        /// Deletes a training schedule by its ID and its related instructor assignment.
        /// If the related assignment is Approved, changes status to Deleting and creates a request.
        /// </summary>
        public async Task<bool> DeleteTrainingScheduleAsync(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.Subject
            );
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");

            // Delete related instructor assignment (if any)
            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.SubjectId == schedule.SubjectID
            );
            if (assignment != null)
            {
                _unitOfWork.InstructorAssignmentRepository.DeleteAsync(assignment.AssignmentId);
            }

            // Delete schedule directly
            _unitOfWork.TrainingScheduleRepository.DeleteAsync(scheduleId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        /// <summary>
        /// Generates a ScheduleID in the format SCD-XXXXXX where XXXXXX is a random 6-digit number.
        /// </summary>
        private string GenerateScheduleId()
        {
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // Get first 6 characters
            return $"SCD-{guidPart}";
        }
        private async Task ValidateTrainingScheduleAsync(TrainingScheduleDTO dto, string scheduleId = null)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectID);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectID} does not exist.");

            // Validate InstructorID
            var instructor = await _unitOfWork.UserRepository.GetAsync(
                u => u.UserId.Equals(dto.InstructorID),
                u => u.Role
            );
            if (instructor == null)
                throw new ArgumentException($"Instructor with ID {dto.InstructorID} does not exist.");
            if (instructor.RoleId == null || instructor.RoleId != 5)
                throw new ArgumentException($"User with ID {dto.InstructorID} is not an Instructor.");

            // Validate DaysOfWeek
            if (dto.DaysOfWeek != null)
            {
                foreach (var day in dto.DaysOfWeek)
                {
                    if (day < 0 || day > 6)
                        throw new ArgumentException($"Invalid day of week value: {day}. Must be between 0 (Sunday) and 6 (Saturday).");
                }
            }

            // Validate ClassTime
            var allowedTimes = new List<TimeOnly>
    {
        new(7, 0),  new(8, 0),  new(9, 0), new(11, 0), new(12, 0), new(13, 0),
        new(14, 0), new(15, 0), new(16, 0), new(17, 0), new(18, 0), new(19, 0), new(20, 0)
    };

            if (!allowedTimes.Contains(dto.ClassTime))
            {
                throw new ArgumentException(
                    $"ClassTime must be one of the following: {string.Join(", ", allowedTimes.Select(t => t.ToString("HH:mm")))}. Provided time: {dto.ClassTime:HH:mm:ss}."
                );
            }

            // Validate StartDateTime and EndDateTime
            if (dto.StartDay == default)
                throw new ArgumentException("StartDateTime is required.");
            if (dto.EndDay == default)
                throw new ArgumentException("EndDateTime is required.");
            if (dto.StartDay >= dto.EndDay)
                throw new ArgumentException("StartDateTime must be before EndDateTime.");
            if (dto.StartDay < DateTime.UtcNow)
                throw new ArgumentException("StartDateTime cannot be in the past.");

            // Validate for overlapping schedules (excluding current schedule in case of update)
            var existingSchedules = await _unitOfWork.TrainingScheduleRepository
                .GetAllAsync(s => s.Location == dto.Location
                               && s.Room == dto.Room
                               && s.ClassTime == dto.ClassTime);
            // Ensure duration is between 1h20 and 2h50
            var duration = dto.SubjectPeriod;
            if (duration < TimeSpan.FromMinutes(80) || duration > TimeSpan.FromMinutes(170))
            {
                throw new ArgumentException(
                    $"Schedule duration must be between 1 hour 20 minutes and 2 hours 50 minutes. Current duration: {duration.TotalMinutes} minutes.");
            }
            foreach (var existingSchedule in existingSchedules)
            {
                if (scheduleId != null && existingSchedule.ScheduleID == scheduleId)
                    continue; // Ignore self if updating

                // Check overlapping date ranges
                bool isDateOverlapping = dto.StartDay <= existingSchedule.EndDateTime &&
                                          dto.EndDay >= existingSchedule.StartDateTime;

                // Check overlapping days of the week
                var existingDays = existingSchedule.DaysOfWeek?.Select(d => (int)d) ?? new List<int>();
                var newDays = dto.DaysOfWeek ?? new List<int>();
                var overlappingDays = existingDays.Intersect(newDays).ToList();

                if (isDateOverlapping && overlappingDays.Any())
                {
                    throw new ArgumentException(
                        $"A subject is already scheduled in Room '{dto.Room}' at '{dto.Location}' on " +
                        $"{string.Join(", ", overlappingDays.Select(d => ((DayOfWeek)d).ToString()))} " +
                        $"at {dto.ClassTime:HH:mm} during {existingSchedule.StartDateTime:yyyy-MM-dd} to {existingSchedule.EndDateTime:yyyy-MM-dd}."
                    );
                }
            }
        }

    }
}
