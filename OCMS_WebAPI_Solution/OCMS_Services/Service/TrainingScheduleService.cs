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
        private readonly IInstructorAssignmentService _instructorAssignmentService;

        public TrainingScheduleService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            IInstructorAssignmentService instructorAssignmentService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _instructorAssignmentService = instructorAssignmentService ?? throw new ArgumentNullException(nameof(instructorAssignmentService));
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

        /// <summary>
        /// Updates an existing training schedule and its associated instructor assignment.
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

            // Validate InstructorID (assuming it's in the DTO)
            var instructorExists = await _unitOfWork.InstructorAssignmentRepository.ExistsAsync(i => i.InstructorId == dto.InstructorID);
            if (!instructorExists)
                throw new ArgumentException($"Instructor with ID {dto.InstructorID} does not exist.");

            // Map DTO to existing entity, preserving fields not in DTO
            _mapper.Map(dto, schedule);
            schedule.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Update InstructorAssignment
            await ManageInstructorAssignment(dto.SubjectID, dto.InstructorID, schedule.CreatedBy);

            // Fetch with related data
            var updatedSchedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.Subject,
                s => s.Instructor,
                s => s.CreatedByUser
            );
            return _mapper.Map<TrainingScheduleModel>(updatedSchedule);
        }

        // Other methods (GetAll, GetById, Delete) remain unchanged...

        /// <summary>
        /// Manages the instructor assignment (create or update) based on subject and instructor.
        /// </summary>
        private async Task ManageInstructorAssignment(string subjectId, string instructorId, string assignByUserId)
        {
            // Check if an assignment already exists for this subject
            var existingAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.SubjectId == subjectId
            );

            if (existingAssignment == null)
            {
                // Create new assignment if none exists
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
                // Update existing assignment if the instructor has changed
                var assignmentDto = new InstructorAssignmentDTO
                {
                    SubjectId = subjectId,
                    InstructorId = instructorId,
                    Notes = existingAssignment.Notes ?? "Updated from training schedule"
                };
                await _instructorAssignmentService.UpdateInstructorAssignmentAsync(existingAssignment.AssignmentId, assignmentDto);
            }
            // If the instructor is the same, no update is needed
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
