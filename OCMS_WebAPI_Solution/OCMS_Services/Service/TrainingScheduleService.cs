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
                s => s.Instructor,
                s => s.CreatedBy,
                s => s.CourseSubjectSpecialty,
                s => s.CourseSubjectSpecialty.Subject
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
                s => s.Instructor,
                s => s.CreatedBy,
                s => s.CourseSubjectSpecialty,
                s => s.CourseSubjectSpecialty.Subject
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
            var css = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAsync(
                css => css.Id == dto.CourseSubjectSpecialtyId,
                css => css.Course,
                css => css.Subject,
                css => css.Specialty,
                css => css.Course.TrainingPlans
            );

            if (css == null)
                throw new ArgumentException($"CourseSubjectSpecialty with ID '{dto.CourseSubjectSpecialtyId}' not found.");

            var instructor = await _unitOfWork.UserRepository.GetAsync(u => u.UserId == dto.InstructorID);
            if (instructor == null)
                throw new ArgumentException($"Instructor with ID '{dto.InstructorID}' not found.");

            if (css.SpecialtyId != instructor.SpecialtyId)
                throw new ArgumentException($"Instructor's specialty does not match the specialty of CourseSubjectSpecialty '{dto.CourseSubjectSpecialtyId}' or its training plan.");

            if (dto.StartDay >= dto.EndDay)
                throw new ArgumentException("StartDay must be before EndDay.");
            await ValidateTrainingScheduleAsync(dto);
            var schedule = _mapper.Map<TrainingSchedule>(dto);
            schedule.ScheduleID = GenerateScheduleId();
            schedule.CourseSubjectSpecialtyId = dto.CourseSubjectSpecialtyId; // Ensure this is explicitly set
            schedule.CreatedByUserId = createdByUserId;
            schedule.CreatedDate = DateTime.UtcNow;
            schedule.ModifiedDate = DateTime.UtcNow;
            schedule.Status = ScheduleStatus.Pending;

            await _unitOfWork.TrainingScheduleRepository.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Assign instructor after creating schedule
            await ManageInstructorAssignment(dto.CourseSubjectSpecialtyId, dto.InstructorID, createdByUserId);

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
            var instructor = await _unitOfWork.UserRepository.GetByIdAsync(dto.InstructorID);
            // Apply update
            _mapper.Map(dto, schedule);
            schedule.Instructor = instructor;
            schedule.ModifiedDate = DateTime.Now;
            


            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            // Update InstructorAssignment if needed
            await ManageInstructorAssignment(dto.CourseSubjectSpecialtyId, dto.InstructorID, schedule.CreatedByUserId);

            var updatedSchedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.Instructor,
                s => s.CreatedBy
            );

            return _mapper.Map<TrainingScheduleModel>(updatedSchedule);
        }
        #endregion

        #region Manage Instructor Assignment
        /// <summary>
        /// Manages the instructor assignment (create or update) based on subject and instructor.
        /// Ensures the instructor has a matching specialty with the course/training plan.
        /// </summary>
        public async Task ManageInstructorAssignment(string courseSubjectSpecialtyId, string instructorId, string assignByUserId)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(courseSubjectSpecialtyId))
                throw new ArgumentException("CourseSubjectSpecialtyId cannot be null or empty", nameof(courseSubjectSpecialtyId));

            if (string.IsNullOrEmpty(instructorId))
                throw new ArgumentException("InstructorId cannot be null or empty", nameof(instructorId));

            if (string.IsNullOrEmpty(assignByUserId))
                throw new ArgumentException("AssignByUserId cannot be null or empty", nameof(assignByUserId));

            // Kiểm tra Specialty của Instructor và Subject/Course/TrainingPlan
            var instructor = await _unitOfWork.UserRepository.GetAsync(u => u.UserId == instructorId);
            var courseSubjectSpecialty = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAsync(
                s => s.Id == courseSubjectSpecialtyId,
                s => s.Course,
                s => s.Course.TrainingPlans);

            if (instructor == null)
                throw new ArgumentException($"Instructor with ID '{instructorId}' not found");

            if (courseSubjectSpecialty == null)
                throw new ArgumentException($"CourseSubjectSpecialty with ID '{courseSubjectSpecialtyId}' not found");

            string trainingPlanSpecialtyId = courseSubjectSpecialty.SpecialtyId;

            // Kiểm tra nếu Instructor có Specialty phù hợp
            if (instructor.SpecialtyId != trainingPlanSpecialtyId)
            {
                throw new InvalidOperationException("Instructor's specialty does not match with the Training Plan's specialty");
            }

            // Tiếp tục với logic assign instructor hiện tại
            var existingAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.CourseSubjectSpecialtyId == courseSubjectSpecialtyId
            );

            if (existingAssignment == null)
            {
                var assignmentDto = new InstructorAssignmentDTO
                {
                    CourseSubjectSpecialtyId = courseSubjectSpecialtyId,  // Ensure this is set correctly
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
                    CourseSubjectSpecialtyId = courseSubjectSpecialtyId,  // Ensure this is set correctly
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
            i => i.CourseSubjectSpecialty,
            i => i.CourseSubjectSpecialty.Subject,
            i => i.CourseSubjectSpecialty.Schedules
                });
            if (assignments == null || !assignments.Any())
                throw new InvalidOperationException("No subject assignments found for this instructor.");

            var result = assignments
                .GroupBy(a => a.CourseSubjectSpecialty)
                .Select(group => new InstructorSubjectScheduleModel
                {
                    SubjectId = group.Key.SubjectId,
                    SubjectName = group.Key.Subject.SubjectName,
                    Description = group.Key.Subject.Description,
                    Schedules = group.Key.Schedules != null
                        ? group.Key.Schedules
                            .Where(s => s.InstructorID == instructorId)
                            .Select(s => new TrainingScheduleModel
                            {
                                ScheduleID = s.ScheduleID,
                                CourseSubjectSpecialtyId = s.CourseSubjectSpecialtyId,
                                SubjectId = s.CourseSubjectSpecialty.SubjectId,
                                SubjectName = s.CourseSubjectSpecialty.Subject.SubjectName,
                                DaysOfWeek = string.Join(",", s.DaysOfWeek),
                                InstructorID = s.InstructorID,
                                InstructorName = s.Instructor?.FullName,
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
                .Where(ta => ta.CourseSubjectSpecialty != null && ta.CourseSubjectSpecialty.Subject != null)
                .Select(ta => new TraineeSubjectScheduleModel
                {
                    SubjectId = ta.CourseSubjectSpecialty.Subject.SubjectId,
                    SubjectName = ta.CourseSubjectSpecialty.Subject.SubjectName,
                    Description = ta.CourseSubjectSpecialty.Subject.Description,
                    Schedules = ta.CourseSubjectSpecialty.Schedules?
                        .Select(s => new TrainingScheduleModel
                        {
                            ScheduleID = s.ScheduleID,
                            CourseSubjectSpecialtyId = s.CourseSubjectSpecialtyId,
                            SubjectId = s.CourseSubjectSpecialty.SubjectId,
                            SubjectName = s.CourseSubjectSpecialty.Subject.SubjectName,
                            Notes = s.Notes,
                            DaysOfWeek = string.Join(",", s.DaysOfWeek),
                            InstructorID = s.InstructorID,
                            InstructorName = s.Instructor?.FullName,
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
        /// Deletes a training schedule by its ID and its related instructor assignment.
        /// If the related assignment is Approved, changes status to Deleting and creates a request.
        /// </summary>
        public async Task<bool> DeleteTrainingScheduleAsync(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
                throw new ArgumentException("Schedule ID cannot be null or empty.", nameof(scheduleId));

            var schedule = await _unitOfWork.TrainingScheduleRepository.GetAsync(
                s => s.ScheduleID == scheduleId,
                s => s.CourseSubjectSpecialty
            );
            if (schedule == null)
                throw new KeyNotFoundException($"Training schedule with ID {scheduleId} not found.");
            if (schedule.Status == ScheduleStatus.Incoming)
            {
                throw new Exception("Schedule is approved. Please send request to delete if needed.");
            }
            // Delete related instructor assignment (if any)
            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.CourseSubjectSpecialtyId == schedule.CourseSubjectSpecialtyId
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

            // Validate CourseSubjectSpecialtyId
            var cssExists = await _unitOfWork.CourseSubjectSpecialtyRepository.ExistsAsync(css => css.Id == dto.CourseSubjectSpecialtyId);
            if (!cssExists)
                throw new ArgumentException($"CourseSubjectSpecialty with ID {dto.CourseSubjectSpecialtyId} does not exist.");

            // Validate only one schedule per CourseSubjectSpecialty
            var cssSchedules = await _unitOfWork.TrainingScheduleRepository
                .GetAllAsync(s => s.CourseSubjectSpecialtyId == dto.CourseSubjectSpecialtyId);

            if (scheduleId == null && cssSchedules.Any())
            {
                // Creating new schedule but one already exists
                throw new ArgumentException($"CourseSubjectSpecialty with ID {dto.CourseSubjectSpecialtyId} already has a schedule. Only one schedule is allowed per CourseSubjectSpecialty.");
            }
            else if (scheduleId != null && cssSchedules.Any(s => s.ScheduleID != scheduleId))
            {
                // Updating, but another schedule exists for this CourseSubjectSpecialty
                throw new ArgumentException($"Another schedule already exists for CourseSubjectSpecialty ID {dto.CourseSubjectSpecialtyId}. Only one schedule is allowed per CourseSubjectSpecialty.");
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

            // Get Course and TrainingPlan to validate dates
            var css = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAsync(
                css => css.Id == dto.CourseSubjectSpecialtyId,
                css => css.Course,
                css => css.Course.TrainingPlans
            );
            if (css == null)
                throw new ArgumentException($"CourseSubjectSpecialty with ID {dto.CourseSubjectSpecialtyId} does not exist.");

            var course = css.Course;
            
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
                        $"A schedule is already booked in Room '{dto.Room}' at '{dto.Location}' on " +
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
