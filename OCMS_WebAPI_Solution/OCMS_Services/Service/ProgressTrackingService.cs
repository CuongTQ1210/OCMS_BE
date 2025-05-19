using Microsoft.Extensions.Logging;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Service for tracking and updating the progress status of educational entities:
/// ClassSubject, Course, and other relevant entities.
/// </summary>

namespace OCMS_Services.Service
{
    public class ProgressTrackingService : IProgressTrackingService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ITrainingScheduleRepository _scheduleRepository;
        private readonly ILogger<ProgressTrackingService> _logger;

        public ProgressTrackingService(
            UnitOfWork unitOfWork,
            ITrainingScheduleRepository scheduleRepository,
            ILogger<ProgressTrackingService> logger)
        {
            _unitOfWork = unitOfWork;
            _scheduleRepository = scheduleRepository;
            _logger = logger;
        }

        #region Check and update ClassSubject progress
        /// <summary>
        /// Checks and updates the progress status of a ClassSubject based on schedules and grades.
        /// A ClassSubject is considered completed when all schedules have ended and all trainees have grades.
        /// </summary>
        /// <param name="classSubjectId">The ID of the ClassSubject to check</param>
        public async Task CheckAndUpdateClassSubjectStatus(string classSubjectId)
        {
            try
            {
                _logger.LogInformation($"Checking status for ClassSubject ID: {classSubjectId}");

                var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(classSubjectId);
                if (classSubject == null)
                {
                    _logger.LogWarning($"ClassSubject with ID {classSubjectId} not found");
                    return;
                }

                // Get all schedules for this ClassSubject
                var schedules = await _scheduleRepository.GetSchedulesByClassSubjectIdAsync(classSubjectId);
                if (schedules == null || !schedules.Any())
                {
                    _logger.LogWarning($"No schedules found for ClassSubject {classSubjectId}");
                    return;
                }

                // Check if the subject has started yet
                bool hasStarted = schedules.Any(s => DateTime.Now >= s.StartDateTime);
                if (!hasStarted)
                {
                    _logger.LogInformation($"ClassSubject {classSubjectId} has not started yet");
                    return;
                }

                // Check if all schedules have ended
                bool allSchedulesEnded = schedules.All(s =>
                    DateTime.Now > s.EndDateTime && s.Status != ScheduleStatus.Completed);

                if (!allSchedulesEnded)
                {
                    _logger.LogInformation($"ClassSubject {classSubjectId} has schedules that haven't ended yet");
                    return;
                }

                // Get all trainee assignments for this ClassSubject
                var traineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(ta =>
                    ta.ClassSubjectId == classSubjectId);

                if (traineeAssigns == null || !traineeAssigns.Any())
                {
                    _logger.LogWarning($"No trainees assigned to ClassSubject {classSubjectId}");
                    // If there are no trainees assigned, we can still mark schedules as completed
                    // as there might be theoretical subjects with no student attendance required
                    foreach (var schedule in schedules)
                    {
                        if (schedule.Status != ScheduleStatus.Completed && schedule.Status != ScheduleStatus.Canceled)
                        {
                            schedule.Status = ScheduleStatus.Completed;
                            schedule.ModifiedDate = DateTime.Now;
                            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                        }
                    }
                    await _unitOfWork.SaveChangesAsync();

                    // Still check the course status as this might be the last subject
                    await CheckAndUpdateCourseStatus(classSubject.ClassId);
                    return;
                }

                // Get all grades for this ClassSubject
                var grades = await _unitOfWork.GradeRepository.FindAsync(g =>
                    traineeAssigns.Select(ta => ta.TraineeAssignId).Contains(g.TraineeAssignID));

                // Check if all assigned trainees have grades
                bool allTraineesGraded = true;
                foreach (var traineeAssign in traineeAssigns)
                {
                    bool hasGrade = grades.Any(g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
                    if (!hasGrade)
                    {
                        allTraineesGraded = false;
                        _logger.LogInformation($"Trainee assign {traineeAssign.TraineeAssignId} doesn't have grade for ClassSubject {classSubjectId}");
                        break;
                    }
                }

                if (!allTraineesGraded)
                {
                    _logger.LogInformation($"ClassSubject {classSubjectId} has trainees without grades");
                    return;
                }

                // Update all schedules to Completed status
                foreach (var schedule in schedules)
                {
                    if (schedule.Status != ScheduleStatus.Completed && schedule.Status != ScheduleStatus.Canceled)
                    {
                        schedule.Status = ScheduleStatus.Completed;
                        schedule.ModifiedDate = DateTime.Now;
                        await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                        _logger.LogInformation($"Updated Schedule {schedule.ScheduleID} to Completed");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"All schedules for ClassSubject {classSubjectId} marked as Completed");

                // Check and update the related Course status
                await CheckAndUpdateCourseStatus(classSubject.ClassId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking status for ClassSubject {classSubjectId}");
                throw;
            }
        }
        #endregion

        #region Maintain for backward compatibility - CourseSubjectSpecialty
        /// <summary>
        /// Legacy method maintained for backward compatibility.
        /// Redirects to the new ClassSubjectStatus method.
        /// </summary>
        public async Task CheckAndUpdateCourseSubjectSpecialtyStatus(string courseSubjectSpecialtyId)
        {
            // This is a legacy method that now redirects to the new method
            _logger.LogWarning($"CheckAndUpdateCourseSubjectSpecialtyStatus is deprecated. Redirecting to CheckAndUpdateClassSubjectStatus");
            await CheckAndUpdateClassSubjectStatus(courseSubjectSpecialtyId);
        }
        #endregion

        #region Check and update Course progress
        /// <summary>
        /// Checks and updates the Course progress status based on all related ClassSubject completions.
        /// A Course is considered completed when all its ClassSubjects are completed.
        /// This will update the Course.Progress property from Ongoing to Completed when appropriate.
        /// </summary>
        /// <param name="courseId">The ID of the Course to check</param>
        public async Task CheckAndUpdateCourseStatus(string courseId)
        {
            try
            {
                _logger.LogInformation($"Checking status for Course ID: {courseId}");

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    _logger.LogWarning($"Course with ID {courseId} not found");
                    return;
                }

                // If the course is already completed or not yet started, skip
                if (course.Progress == Progress.Completed || course.Progress == Progress.NotYet)
                {
                    _logger.LogInformation($"Course {courseId} is already {course.Progress}, no update needed");
                    return;
                }

                // Get all ClassSubject entries for this course
                var classSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(cs =>
                    cs.ClassId == courseId);

                if (classSubjects == null || !classSubjects.Any())
                {
                    _logger.LogWarning($"No ClassSubjects found for Course {courseId}");
                    return;
                }

                // Check if all ClassSubjects are completed
                bool allClassSubjectsCompleted = true;
                foreach (var classSubject in classSubjects)
                {
                    // Get all schedules for this ClassSubject
                    var schedules = await _scheduleRepository.GetSchedulesByClassSubjectIdAsync(classSubject.ClassSubjectId);

                    if (schedules == null || !schedules.Any())
                    {
                        allClassSubjectsCompleted = false;
                        _logger.LogInformation($"ClassSubject {classSubject.ClassSubjectId} has no schedules");
                        break;
                    }

                    bool subjectCompleted = schedules.All(s =>
                        s.Status == ScheduleStatus.Completed || s.Status == ScheduleStatus.Canceled);

                    if (!subjectCompleted)
                    {
                        allClassSubjectsCompleted = false;
                        _logger.LogInformation($"ClassSubject {classSubject.ClassSubjectId} has incomplete schedules");
                        break;
                    }

                    // Check if all trainees have grades
                    var traineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(ta =>
                        ta.ClassSubjectId == classSubject.ClassSubjectId);

                    var grades = await _unitOfWork.GradeRepository.FindAsync(g =>
                        traineeAssigns.Select(ta => ta.TraineeAssignId).Contains(g.TraineeAssignID));

                    foreach (var traineeAssign in traineeAssigns)
                    {
                        bool hasGrade = grades.Any(g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
                        if (!hasGrade)
                        {
                            allClassSubjectsCompleted = false;
                            _logger.LogInformation($"Trainee {traineeAssign.TraineeId} has no grade for ClassSubject {classSubject.ClassSubjectId}");
                            break;
                        }
                    }

                    if (!allClassSubjectsCompleted)
                        break;
                }

                if (allClassSubjectsCompleted)
                {
                    course.Progress = Progress.Completed;
                    course.UpdatedAt = DateTime.Now;
                    await _unitOfWork.CourseRepository.UpdateAsync(course);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated Course {courseId} to Completed");

                    // Check and update all related TrainingPlans
                    // Note: TrainingPlan entity has been removed according to comments
                }
                else
                {
                    _logger.LogInformation($"Course {courseId} has incomplete subjects");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking status for Course {courseId}");
                throw;
            }
        }
        #endregion

        #region Check and update TrainingPlan progress
        /// <summary>
        /// Checks and updates the TrainingPlan status based on the related Course progress.
        /// A TrainingPlan is considered completed when its associated Course is completed.
        /// This will update the TrainingPlanStatus from Approved/Pending to Completed when appropriate.
        /// </summary>
        /// <param name="planId">The ID of the TrainingPlan to check</param>
        public async Task CheckAndUpdateTrainingPlanStatus(string planId)
        {
            // Method disabled as TrainingPlan entity has been removed
            await Task.CompletedTask;
        }
        #endregion

        #region System-wide progress check
        /// <summary>
        /// Performs a system-wide check and update of all educational entities' progress statuses.
        /// This method:
        /// 1. Updates schedule statuses (Approved → Incoming)
        /// 2. Processes completed ClassSubjects
        /// 3. Updates Course progress (Ongoing → Completed)
        /// </summary>
        public async Task CheckAndUpdateAllStatuses()
        {
            try
            {
                _logger.LogInformation("Starting system-wide status check");

                // 1. Check schedules that should be marked as Incoming (approved and past start time)
                var startingSchedules = await _unitOfWork.TrainingScheduleRepository.FindAsync(
                    s => s.Status == ScheduleStatus.Approved && s.StartDateTime <= DateTime.Now);

                foreach (var schedule in startingSchedules)
                {
                    schedule.Status = ScheduleStatus.Incoming;
                    schedule.ModifiedDate = DateTime.Now;
                    await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                    _logger.LogInformation($"Updated Schedule {schedule.ScheduleID} to Incoming");
                }

                if (startingSchedules.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated {startingSchedules.Count()} schedules to Incoming status");
                }

                // 2. Check schedules that have ended but not yet marked as completed
                var expiredSchedules = await _unitOfWork.TrainingScheduleRepository.FindAsync(
                    s => s.Status == ScheduleStatus.Incoming && s.EndDateTime < DateTime.Now);

                var processedClassSubjectIds = new HashSet<string>();

                foreach (var schedule in expiredSchedules)
                {
                    // Process each ClassSubject only once
                    if (!processedClassSubjectIds.Contains(schedule.ClassSubjectId))
                    {
                        await CheckAndUpdateClassSubjectStatus(schedule.ClassSubjectId);
                        processedClassSubjectIds.Add(schedule.ClassSubjectId);
                    }
                }

                _logger.LogInformation($"Processed {processedClassSubjectIds.Count} ClassSubjects with expired schedules");

                // 3. Check ongoing courses
                var ongoingCourses = await _unitOfWork.CourseRepository.FindAsync(
                    c => c.Progress == Progress.Ongoing && c.Status == CourseStatus.Approved);

                foreach (var course in ongoingCourses)
                {
                    await CheckAndUpdateCourseStatus(course.CourseId);
                }

                _logger.LogInformation($"Processed {ongoingCourses.Count()} ongoing courses");
                _logger.LogInformation("System-wide status check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during system-wide status check");
                throw;
            }
        }
        #endregion
    }
}
