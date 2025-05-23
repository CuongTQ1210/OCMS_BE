using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class TrainingScheduleService : ITrainingScheduleService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;
        private readonly IInstructorAssignmentService _instructorAssignmentService;
        private readonly IRequestService _requestService;
        
        public TrainingScheduleService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            IInstructorAssignmentService instructorAssignmentService,
            IRequestService requestService,
            ITrainingScheduleRepository trainingScheduleRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _instructorAssignmentService = instructorAssignmentService ?? throw new ArgumentNullException(nameof(instructorAssignmentService));
            _requestService = requestService;
            _trainingScheduleRepository = trainingScheduleRepository ?? throw new ArgumentNullException(nameof(trainingScheduleRepository));
        }

        #region Get All Training Schedules
        /// <summary>
        /// Retrieves all training schedules with related Subject, Instructor, and CreatedByUser data.
        /// </summary>
        public async Task<IEnumerable<TrainingScheduleModel>> GetAllTrainingSchedulesAsync()
        {
            var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(
                s => s.CreatedBy,
                s => s.ClassSubject,
                s => s.ClassSubject.SubjectSpecialty,
                s => s.ClassSubject.SubjectSpecialty.Subject,
                s => s.ClassSubject.InstructorAssignment,
                s => s.ClassSubject.InstructorAssignment.Instructor
            );
            return _mapper.Map<IEnumerable<TrainingScheduleModel>>(schedules);
        }
        #endregion

        #region Get All Training Schedules By Id
        /// <summary>
        /// Retrieves a training schedule by its ID, including related data.
        /// </summary>
        public async Task<TrainingScheduleModel> GetTrainingScheduleByIdAsync(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.CreatedBy,
                s => s.ClassSubject,
                s => s.ClassSubject.SubjectSpecialty,
                s => s.ClassSubject.SubjectSpecialty.Subject,
                s => s.ClassSubject.InstructorAssignment,
                s => s.ClassSubject.InstructorAssignment.Instructor
            );
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");

            return _mapper.Map<TrainingScheduleModel>(schedule);
        }
        #endregion

        #region Create Training Schedule
        /// <summary>
        /// Creates a new training schedule. Requires that an instructor assignment already exists.
        /// </summary>
        public async Task<TrainingScheduleModel> CreateTrainingScheduleAsync(TrainingScheduleDTO dto, string createdByUserId)
        {
            var classSubject = await _unitOfWork.ClassSubjectRepository.GetAsync(
                cs => cs.ClassSubjectId == dto.ClassSubjectId,
                cs => cs.Class,
                cs => cs.SubjectSpecialty,
                cs => cs.SubjectSpecialty.Subject         
            );

            if (classSubject == null)
                throw new ArgumentException($"ClassSubject with ID '{dto.ClassSubjectId}' not found.");
            var instructorAssignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(classSubject.InstructorAssignmentID);
            var instructor =  await _unitOfWork.UserRepository.GetByIdAsync(instructorAssignment.InstructorId);
            if (instructor == null)
                throw new ArgumentException($"Instructor with ID '{instructorAssignment.InstructorId}' not found.");

            // Verify that an instructor assignment already exists for this subject and instructor
            var existingAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.SubjectId == classSubject.SubjectSpecialty.SubjectId && a.InstructorId == instructor.UserId
            );

            // Get the subject specialty through the SubjectSpecialty entity
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                ss => ss.SubjectId == classSubject.SubjectSpecialty.SubjectId);
                
            if (subjectSpecialty == null)
                throw new ArgumentException($"No specialty found for Subject with ID '{classSubject.SubjectSpecialty.SubjectId}'.");

            if (dto.StartDay >= dto.EndDay)
                throw new ArgumentException("StartDay must be before EndDay.");
                
            await ValidateTrainingScheduleAsync(dto);
            
            var schedule = _mapper.Map<TrainingSchedule>(dto);
            schedule.ScheduleID = GenerateScheduleId();
            schedule.ClassSubjectId = dto.ClassSubjectId; // Ensure this is explicitly set
            schedule.CreatedByUserId = createdByUserId;
            schedule.CreatedDate = DateTime.UtcNow;
            schedule.ModifiedDate = DateTime.UtcNow;
            schedule.Status = ScheduleStatus.Pending;

            await _unitOfWork.TrainingScheduleRepository.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Create approval request for the schedule
            var requestDto = new RequestDTO
            {
                RequestEntityId = schedule.ScheduleID,
                RequestType = RequestType.ClassSchedule,
                Description = $"Request to approve schedule for class subject {dto.ClassSubjectId}",
                Notes = $"Schedule details:\n" +
                       $"Location: {schedule.Location}\n" +
                       $"Room: {schedule.Room}\n" +
                       $"Start Day: {schedule.StartDateTime}\n" +
                       $"End Day: {schedule.EndDateTime}\n" +
                       $"Days of Week: {schedule.DaysOfWeek}\n" +
                       $"Class Time: {schedule.ClassTime}\n" +
                       $"Subject Period: {schedule.SubjectPeriod}\n" +
                       $"Notes: {schedule.Notes}"
            };
            await _requestService.CreateRequestAsync(requestDto, createdByUserId);

            return _mapper.Map<TrainingScheduleModel>(schedule);
        }
        #endregion

        #region Update Training Schedule
        public async Task<TrainingScheduleModel> UpdateTrainingScheduleAsync(string scheduleId, TrainingScheduleDTO dto)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");
            if(schedule.Status == ScheduleStatus.Incoming)
            {
                throw new Exception("Schedule is approved. Please send request to update if needed.");
            }    
            await ValidateTrainingScheduleAsync(dto, scheduleId);
            
            // Get class subject and verify instructor assignment
            var classSubject = await _unitOfWork.ClassSubjectRepository.GetAsync(
                cs => cs.ClassSubjectId == dto.ClassSubjectId,
                cs => cs.Class,
                cs => cs.SubjectSpecialty,
                cs => cs.SubjectSpecialty.Subject
            );

            if (classSubject == null)
                throw new ArgumentException($"ClassSubject with ID '{dto.ClassSubjectId}' not found.");

            var instructorAssignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(classSubject.InstructorAssignmentID);
            if (instructorAssignment == null)
                throw new ArgumentException($"No instructor assignment found for ClassSubject '{dto.ClassSubjectId}'.");

            var instructor = await _unitOfWork.UserRepository.GetByIdAsync(instructorAssignment.InstructorId);
            if (instructor == null)
                throw new ArgumentException($"Instructor with ID '{instructorAssignment.InstructorId}' not found.");

            // Get the subject specialty through the SubjectSpecialty entity
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                ss => ss.SubjectId == classSubject.SubjectSpecialty.SubjectId);
                
            if (subjectSpecialty == null)
                throw new ArgumentException($"No specialty found for Subject with ID '{classSubject.SubjectSpecialty.SubjectId}'.");

            
            // Apply update
            _mapper.Map(dto, schedule);
            schedule.ModifiedDate = DateTime.Now;

            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            var updatedSchedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.CreatedBy,
                s => s.ClassSubject,
                s => s.ClassSubject.SubjectSpecialty,
                s => s.ClassSubject.SubjectSpecialty.Subject);

            return _mapper.Map<TrainingScheduleModel>(updatedSchedule);
        }
        #endregion

        #region Manage Instructor Assignment
        /// <summary>
        /// Manages the instructor assignment (create or update) based on subject and instructor.
        /// Ensures the instructor has a matching specialty with the course/training plan.
        /// </summary>
        public async Task ManageInstructorAssignment(string classSubjectId, string instructorId, string assignByUserId)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(classSubjectId))
                throw new ArgumentException("ClassSubjectId cannot be null or empty", nameof(classSubjectId));

            if (string.IsNullOrEmpty(instructorId))
                throw new ArgumentException("InstructorId cannot be null or empty", nameof(instructorId));

            if (string.IsNullOrEmpty(assignByUserId))
                throw new ArgumentException("AssignByUserId cannot be null or empty", nameof(assignByUserId));

            // Check specialty compatibility between instructor and subject
            var instructor = await _unitOfWork.UserRepository.GetAsync(u => u.UserId == instructorId);
            var classSubject = await _unitOfWork.ClassSubjectRepository.GetAsync(
                cs => cs.ClassSubjectId == classSubjectId,
                cs => cs.Class,
                cs => cs.SubjectSpecialty,
                cs => cs.SubjectSpecialty.Subject);

            if (instructor == null)
                throw new ArgumentException($"Instructor with ID '{instructorId}' not found");

            if (classSubject == null)
                throw new ArgumentException($"ClassSubject with ID '{classSubjectId}' not found");

            // Get the subject specialty through the SubjectSpecialty entity
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                ss => ss.SubjectId == classSubject.SubjectSpecialty.SubjectId);
                
            if (subjectSpecialty == null)
                throw new ArgumentException($"No specialty found for Subject with ID '{classSubject.SubjectSpecialty.SubjectId}'.");


            // Proceed with logic to assign instructor
            var existingAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.SubjectId == classSubject.SubjectSpecialty.SubjectId
            );

            if (existingAssignment == null)
            {
                var assignmentDto = new InstructorAssignmentDTO
                {
                    SubjectId = classSubject.SubjectSpecialty.SubjectId,
                    InstructorId = instructorId,
                    Notes = "Automatically created from training schedule"
                };

                try
                {
                    await _instructorAssignmentService.CreateInstructorAssignmentAsync(assignmentDto, assignByUserId);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to create instructor assignment: {ex.Message}", ex);
                }
            }
            else if (existingAssignment.InstructorId != instructorId)
            {
                var assignmentDto = new InstructorAssignmentDTO
                {
                    SubjectId = classSubject.SubjectSpecialty.SubjectId,
                    InstructorId = instructorId,
                    Notes = existingAssignment.Notes ?? "Updated from training schedule"
                };

                try
                {
                    await _instructorAssignmentService.UpdateInstructorAssignmentAsync(existingAssignment.AssignmentId, assignmentDto);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to update instructor assignment: {ex.Message}", ex);
                }
            }
        }
        #endregion

        #region Get Subjects and Schedules for Instructor
        public async Task<List<InstructorSubjectScheduleModel>> GetSubjectsAndSchedulesForInstructorAsync(string instructorId)
        {
            if (string.IsNullOrEmpty(instructorId))
                throw new ArgumentException("Instructor ID cannot be null or empty.");

            
            var assignments = await _unitOfWork.InstructorAssignmentRepository.GetAllAsync(
                predicate: i => i.InstructorId == instructorId,
                includes: new Expression<Func<InstructorAssignment, object>>[]
                {
                    i => i.Subject,
                    i => i.Instructor
                });
                
            if (assignments == null || !assignments.Any())
                throw new InvalidOperationException("No subject assignments found for this instructor.");

            // Get the subjects assigned to this instructor
            var subjectIds = assignments.Select(a => a.SubjectId).ToList();

            // Get class subjects related to these subjects
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                cs => subjectIds.Contains(cs.SubjectSpecialty.SubjectId),
                cs => cs.SubjectSpecialty,
                cs => cs.SubjectSpecialty.Subject,
                cs => cs.InstructorAssignment,
                cs => cs.InstructorAssignment.Instructor
            );

            // Get schedules for these class subjects
            var classSubjectIds = classSubjects.Select(cs => cs.ClassSubjectId).ToList();
            var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(
                s => classSubjectIds.Contains(s.ClassSubjectId),
                s => s.ClassSubject,
                s => s.ClassSubject.SubjectSpecialty,
                s => s.ClassSubject.SubjectSpecialty.Subject,
                s => s.ClassSubject.InstructorAssignment,
                s => s.ClassSubject.InstructorAssignment.Instructor
            );

            var result = classSubjects
            .GroupBy(cs => cs.SubjectSpecialty.Subject)
            .Select(group => new InstructorSubjectScheduleModel
            {
                SubjectId = group.Key.SubjectId,
                SubjectName = group.Key.SubjectName,
                Description = group.Key.Description,
                Schedules = schedules
                    .Where(s => group.Any(cs => cs.ClassSubjectId == s.ClassSubjectId))
                    .Select(s => new TrainingScheduleModel
                    {
                        ScheduleID = s.ScheduleID,
                        ClassSubjectId = s.ClassSubjectId,
                        SubjectId = group.Key.SubjectId,
                        SubjectName = group.Key.SubjectName,
                        DaysOfWeek = string.Join(",", s.DaysOfWeek),
                        InstructorID = instructorId,
                        InstructorName = assignments.FirstOrDefault(a => a.SubjectId == group.Key.SubjectId)?.Instructor?.FullName,
                        SubjectPeriod = s.SubjectPeriod,
                        ClassTime = s.ClassTime,
                        StartDateTime = s.StartDateTime,
                        EndDateTime = s.EndDateTime,
                        Location = s.Location,
                        Room = s.Room,
                        Status = s.Status.ToString(),
                    })
                    .ToList()
            })
            .ToList();

            if (!result.Any())
                throw new InvalidOperationException("No valid schedules found for this instructor's assigned subjects.");

            return result;
        }
        #endregion

        #region Get Subjects and Schedules for Trainee
        public async Task<List<TraineeSubjectScheduleModel>> GetSubjectsAndSchedulesForTraineeAsync(string traineeId)
        {
            // Get trainee assignments
            var traineeAssignments = await _unitOfWork.TraineeAssignRepository
                .GetAllAsync(ta => ta.TraineeId == traineeId);

            if (traineeAssignments == null || !traineeAssignments.Any())
                throw new InvalidOperationException("No course assignments found for this trainee.");

            // Get class subjects for these assignments
            var classSubjectIds = traineeAssignments.Select(ta => ta.ClassSubjectId).ToList();
            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                cs => classSubjectIds.Contains(cs.ClassSubjectId), // Fixed variable name
                cs => cs.SubjectSpecialty,
                cs => cs.SubjectSpecialty.Subject,
                cs => cs.InstructorAssignment,
                cs => cs.InstructorAssignment.Instructor
            );

            // Get schedules for these class subjects
            var schedules = await _unitOfWork.TrainingScheduleRepository.GetAllAsync(
                s => classSubjectIds.Contains(s.ClassSubjectId),
                s => s.ClassSubject,
                s => s.ClassSubject.SubjectSpecialty,
                s => s.ClassSubject.SubjectSpecialty.Subject,
                s => s.ClassSubject.InstructorAssignment,
                s => s.ClassSubject.InstructorAssignment.Instructor);

            var result = classSubjects
                .Select(cs => new TraineeSubjectScheduleModel
                {
                    SubjectId = cs.SubjectSpecialty.SubjectId,
                    SubjectName = cs.SubjectSpecialty.Subject.SubjectName,
                    Description = cs.SubjectSpecialty.Subject.Description,
                    Schedules = schedules
                        .Where(s => s.ClassSubjectId == cs.ClassSubjectId)
                        .Select(s => new TrainingScheduleModel
                        {
                            ScheduleID = s.ScheduleID,
                            ClassSubjectId = s.ClassSubjectId,
                            SubjectId = cs.SubjectSpecialty.SubjectId,
                            SubjectName = cs.SubjectSpecialty.Subject.SubjectName,
                            Notes = s.Notes,
                            DaysOfWeek = string.Join(",", s.DaysOfWeek),
                            InstructorID = cs.InstructorAssignment?.Instructor?.UserId,
                            InstructorName = cs.InstructorAssignment?.Instructor?.FullName,
                            SubjectPeriod = s.SubjectPeriod,
                            ClassTime = s.ClassTime,
                            StartDateTime = s.StartDateTime,
                            EndDateTime = s.EndDateTime,
                            Location = s.Location,
                            Room = s.Room,
                            Status = s.Status.ToString()
                        }).ToList() ?? new List<TrainingScheduleModel>()
                }).ToList();

            if (!result.Any())
                throw new InvalidOperationException("No subjects and schedules found for this trainee.");

            return result;
        }
        #endregion


        #region Delete Training Schedule
        /// <summary>
        /// Deletes a training schedule by its ID. Instructor assignments are managed separately.
        /// </summary>
        public async Task<bool> DeleteTrainingScheduleAsync(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.ClassSubject,
                s => s.ClassSubject.SubjectSpecialty,
                s => s.ClassSubject.SubjectSpecialty.Subject
            );
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");
            if (schedule.Status == ScheduleStatus.Incoming)
            {
                throw new Exception("Schedule is approved. Please send request to delete if needed.");
            }

            // Delete schedule directly
            await _unitOfWork.TrainingScheduleRepository.DeleteAsync(scheduleId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Helper Methods
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

            // Validate Location
            if (!Enum.IsDefined(typeof(Location), dto.Location))
                throw new ArgumentException($"Invalid location: {dto.Location}. Must be either SectionA or SectionB.");

            // Validate Room
            if (!Enum.IsDefined(typeof(Room), dto.Room))
                throw new ArgumentException($"Invalid room: {dto.Room}. Must be a valid room number (e.g., R001, R101, etc.).");

            // Validate ClassSubjectId
            var classSubjectExists = await _unitOfWork.ClassSubjectRepository.ExistsAsync(cs => cs.ClassSubjectId == dto.ClassSubjectId);
            if (!classSubjectExists)
                throw new ArgumentException($"ClassSubject with ID {dto.ClassSubjectId} does not exist.");

            // Validate only one schedule per ClassSubject
            var classSubjectSchedules = await _unitOfWork.TrainingScheduleRepository
                .GetAllAsync(s => s.ClassSubjectId == dto.ClassSubjectId);

            if (scheduleId == null && classSubjectSchedules.Any())
            {
                // Creating new schedule but one already exists
                throw new ArgumentException($"ClassSubject with ID {dto.ClassSubjectId} already has a schedule. Only one schedule is allowed per ClassSubject.");
            }
            else if (scheduleId != null && classSubjectSchedules.Any(s => s.ScheduleID != scheduleId))
            {
                // Updating, but another schedule exists for this ClassSubject
                throw new ArgumentException($"Another schedule already exists for ClassSubject ID {dto.ClassSubjectId}. Only one schedule is allowed per ClassSubject.");
            }

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

            // Validate StartDay and EndDay
            if (dto.StartDay == default)
                throw new ArgumentException("StartDay is required.");
            if (dto.EndDay == default)
                throw new ArgumentException("EndDay is required.");
            if (dto.StartDay >= dto.EndDay)
                throw new ArgumentException("StartDay must be before EndDay.");
            if (dto.StartDay < DateTime.UtcNow)
                throw new ArgumentException("StartDay cannot be in the past.");
            
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

                bool isDayOverlapping = existingDays.Intersect(newDays).Any();

                if (isDateOverlapping && isDayOverlapping)
                {
                    throw new ArgumentException(
                        $"Schedule overlaps with an existing schedule (ID: {existingSchedule.ScheduleID}) " +
                        $"at the same location ({dto.Location}, Room {dto.Room}) " +
                        $"on the same day(s) of the week at {dto.ClassTime}.");
                }
            }
        }
        #endregion
    }
}
