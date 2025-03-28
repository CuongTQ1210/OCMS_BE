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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrEmpty(createdByUserId))
                throw new ArgumentException("CreatedBy user ID cannot be null or empty.", nameof(createdByUserId));

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectID);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectID} does not exist.");

            // Validate InstructorID (assuming it's in the DTO)
            var instructorExists = await _unitOfWork.InstructorAssignmentRepository.ExistsAsync(i => i.InstructorId == dto.InstructorID);
            if (!instructorExists)
                throw new ArgumentException($"Instructor with ID {dto.InstructorID} does not exist.");

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
            schedule.ScheduleID = scheduleId;
            schedule.SubjectID= dto.SubjectID;
            schedule.InstructorID = dto.InstructorID;
            schedule.CreatedBy = createdByUserId;
            schedule.CreatedDate = DateTime.UtcNow;
            schedule.ModifiedDate = DateTime.UtcNow;

            // Set default values for fields not in DTO
            schedule.StartDateTime = DateTime.UtcNow; // Adjust as needed
            schedule.EndDateTime = DateTime.UtcNow.AddHours(1); // Adjust as needed

            await _unitOfWork.TrainingScheduleRepository.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Create or update InstructorAssignment
            await ManageInstructorAssignment(dto.SubjectID, dto.InstructorID, createdByUserId);

            // Fetch with related data
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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectID);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectID} does not exist.");

            // Validate InstructorID
            var instructorExists = await _unitOfWork.InstructorAssignmentRepository.ExistsAsync(i => i.InstructorId == dto.InstructorID);
            if (!instructorExists)
                throw new ArgumentException($"Instructor with ID {dto.InstructorID} does not exist.");

            // Apply update directly (No approval request)
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
        private string GenerateAssignmentId()
        {
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // Get first 6 characters
            return $"SCD-{guidPart}";
        }
    }
}
