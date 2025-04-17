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
        private readonly Lazy<ITrainingScheduleService> _trainingScheduleService;
        private readonly Lazy<ITrainingPlanService> _trainingPlanService;
        public TrainingScheduleService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            IInstructorAssignmentService instructorAssignmentService,
            IRequestService requestService,
            Lazy<ITrainingScheduleService> trainingScheduleService,
            Lazy<ITrainingPlanService> trainingPlanService,
            ITrainingScheduleRepository trainingScheduleRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _instructorAssignmentService = instructorAssignmentService ?? throw new ArgumentNullException(nameof(instructorAssignmentService));
            _requestService = requestService;
            _trainingPlanService = trainingPlanService ?? throw new ArgumentNullException(nameof(trainingPlanService));
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
            _trainingScheduleRepository = trainingScheduleRepository ?? throw new ArgumentNullException(nameof(trainingScheduleRepository));
        }

        #region Get All Training Schedules
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
        #endregion

        #region Get All Training Schedules By Subject
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
        #endregion

        #region Create Training Schedule
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
            schedule.CreatedDate = DateTime.Now;
            schedule.ModifiedDate = DateTime.Now;
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

            // Apply update
            _mapper.Map(dto, schedule);
            schedule.ModifiedDate = DateTime.Now;

            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
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

        #region Manage Instructor Assignment
        /// <summary>
        /// Manages the instructor assignment (create or update) based on subject and instructor.
        /// Ensures the instructor has a matching specialty with the course/training plan.
        /// </summary>
        public async Task ManageInstructorAssignment(string subjectId, string instructorId, string assignByUserId)
        {
            // Kiểm tra Specialty của Instructor và Subject/Course/TrainingPlan
            var instructor = await _unitOfWork.UserRepository.GetAsync(u => u.UserId == instructorId);
            var subject = await _unitOfWork.SubjectRepository.GetAsync(
                s => s.SubjectId == subjectId,
                s => s.Course,
                s => s.Course.TrainingPlan);

            if (instructor == null || subject == null)
            {
                throw new ArgumentException("Instructor or Subject not found");
            }

            // Lấy Specialty từ Training Plan (phù hợp nhất vì là cấp cao nhất)
            string trainingPlanSpecialtyId = subject.Course.TrainingPlan.SpecialtyId;

            // Kiểm tra nếu Instructor có Specialty phù hợp
            if (instructor.SpecialtyId != trainingPlanSpecialtyId)
            {
                throw new InvalidOperationException("Instructor's specialty does not match with the Training Plan's specialty");
            }

            // Tiếp tục với logic assign instructor hiện tại
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
            i => i.Subject.Schedules
                });
            if (assignments == null || !assignments.Any())
                throw new InvalidOperationException("No subject assignments found for this instructor.");

            var result = assignments
                .GroupBy(a => a.Subject)
                .Select(group => new InstructorSubjectScheduleModel
                {
                    SubjectId = group.Key.SubjectId,
                    SubjectName = group.Key.SubjectName,
                    Description = group.Key.Description,
                    Schedules = group.Key.Schedules != null
                        ? group.Key.Schedules
                            .Where(s => s.InstructorID == instructorId)
                            .Select(s => new TrainingScheduleModel
                            {
                                ScheduleID = s.ScheduleID,
                                DaysOfWeek = string.Join(",", s.DaysOfWeek), 
                                SubjectPeriod = s.SubjectPeriod,
                                ClassTime = s.ClassTime,
                                StartDateTime = s.StartDateTime,
                                EndDateTime = s.EndDateTime,
                                Location = s.Location,
                                Room = s.Room,
                                Status = s.Status.ToString(),
                            })
                            .ToList()
                        : new List<TrainingScheduleModel>()
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
            var assignments = await _trainingScheduleRepository.GetTraineeAssignmentsWithSchedulesAsync(traineeId);


            if (assignments == null || !assignments.Any())
                throw new InvalidOperationException("No course assignments found for this trainee.");

            var result = assignments
                .Where(ta => ta.Course != null && ta.Course.Subjects != null)
                .SelectMany(ta => ta.Course.Subjects
                    .Where(s => s.Schedules != null))
                .Select(subject => new TraineeSubjectScheduleModel
                {
                    SubjectId = subject.SubjectId,
                    SubjectName = subject.SubjectName,
                    Description = subject.Description,
                    Schedules = subject.Schedules?
                        .Select(s => new TrainingScheduleModel
                        {
                            ScheduleID = s.ScheduleID,
                            DaysOfWeek = string.Join(",", s.DaysOfWeek),
                            SubjectPeriod = s.SubjectPeriod,
                            ClassTime = s.ClassTime,
                            StartDateTime = s.StartDateTime,
                            EndDateTime = s.EndDateTime,
                            Location = s.Location,
                            Room = s.Room,
                            Status = s.Status.ToString(),
                        }).ToList() ?? new List<TrainingScheduleModel>()
                }).ToList();

            if (!result.Any())
                throw new InvalidOperationException("No subjects and schedules found for this trainee.");

            return result;
        }
        #endregion

        #region Delete Training Schedule
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
            if (schedule.Status == ScheduleStatus.Incoming)
            {
                throw new Exception("Schedule is approved. Please send request to delete if needed.");
            }
            // Delete related instructor assignment (if any)
            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.SubjectId == schedule.SubjectID
            );
            if (assignment != null)
            {
                await _unitOfWork.InstructorAssignmentRepository.DeleteAsync(assignment.AssignmentId);
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

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectID);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectID} does not exist.");
            // Validate only one schedule per subject
            var subjectSchedules = await _unitOfWork.TrainingScheduleRepository
                .GetAllAsync(s => s.SubjectID == dto.SubjectID);

            if (scheduleId == null && subjectSchedules.Any())
            {
                // Creating new schedule but one already exists
                throw new ArgumentException($"Subject with ID {dto.SubjectID} already has a schedule. Only one schedule is allowed per subject.");
            }
            else if (scheduleId != null && subjectSchedules.Any(s => s.ScheduleID != scheduleId))
            {
                // Updating, but another schedule exists for this subject
                throw new ArgumentException($"Another schedule already exists for Subject ID {dto.SubjectID}. Only one schedule is allowed per subject.");
            }
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
            //get plan start and end date to validate 
            var subject = await _unitOfWork.SubjectRepository.FirstOrDefaultAsync(s => s.SubjectId == dto.SubjectID);
            var course = await _unitOfWork.CourseRepository.FirstOrDefaultAsync(s => s.CourseId == subject.CourseId);
            var plan = await _unitOfWork.TrainingPlanRepository.FirstOrDefaultAsync(s => s.PlanId == course.TrainingPlanId);
            // Validate StartDateTime and EndDateTime
            if (dto.StartDay == default)
                throw new ArgumentException("StartDateTime is required.");
            if (dto.EndDay == default)
                throw new ArgumentException("EndDateTime is required.");
            if (dto.StartDay >= dto.EndDay)
                throw new ArgumentException("StartDateTime must be before EndDateTime.");
            if (dto.StartDay < DateTime.Now)
                throw new ArgumentException("StartDateTime cannot be in the past.");
            if (dto.StartDay < plan.StartDate || dto.EndDay > plan.EndDate)
                throw new ArgumentException("Start and end dates must be within the training plan's duration.");

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

                // Calculate the time range of the current and existing schedules
                var newStartTime = dto.ClassTime;
                var newEndTime = dto.ClassTime.Add(dto.SubjectPeriod);


                var existingStartTime = existingSchedule.ClassTime;
                var existingEndTime = existingSchedule.ClassTime.Add(existingSchedule.SubjectPeriod);

                // Check if time overlaps
                bool isTimeOverlapping = newStartTime < existingEndTime && newEndTime > existingStartTime;

                if (isDateOverlapping && overlappingDays.Any() && isTimeOverlapping)
                {
                    throw new ArgumentException(
                        $"A subject is already scheduled in Room '{dto.Room}' at '{dto.Location}' on " +
                        $"{string.Join(", ", overlappingDays.Select(d => ((DayOfWeek)d).ToString()))} " +
                        $"from {existingStartTime:hh\\:mm} to {existingEndTime:hh\\:mm} during " +
                        $"{existingSchedule.StartDateTime:yyyy-MM-dd} to {existingSchedule.EndDateTime:yyyy-MM-dd}."
                    );
                }
            }
            // Check if instructor is already teaching at the same ClassTime on the same days
            var instructorSchedules = await _unitOfWork.TrainingScheduleRepository
                .GetAllAsync(s => s.InstructorID == dto.InstructorID
                               && s.ClassTime == dto.ClassTime
                               && s.ScheduleID != scheduleId); // Exclude self if updating

            foreach (var schedule in instructorSchedules)
            {
                bool isDateOverlapping = dto.StartDay <= schedule.EndDateTime &&
                                          dto.EndDay >= schedule.StartDateTime;

                var existingDays = schedule.DaysOfWeek?.Select(d => (int)d) ?? new List<int>();
                var newDays = dto.DaysOfWeek ?? new List<int>();
                var overlappingDays = existingDays.Intersect(newDays).ToList();

                if (isDateOverlapping && overlappingDays.Any())
                {
                    throw new ArgumentException(
                        $"Instructor with ID {dto.InstructorID} already has a class on " +
                        $"{string.Join(", ", overlappingDays.Select(d => ((DayOfWeek)d).ToString()))} " +
                        $"at {dto.ClassTime:HH:mm}, from {schedule.StartDateTime:yyyy-MM-dd} to {schedule.EndDateTime:yyyy-MM-dd}."
                    );
                }
            }
        }
        #endregion
    }
}
