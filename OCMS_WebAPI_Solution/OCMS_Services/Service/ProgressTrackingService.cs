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
using Hangfire;

namespace OCMS_Services.Service
{
    public class ProgressTrackingService : IProgressTrackingService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ITrainingScheduleRepository _scheduleRepository;
        private readonly ILogger<ProgressTrackingService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;


        public ProgressTrackingService(
            UnitOfWork unitOfWork,
            ITrainingScheduleRepository scheduleRepository,
            ILogger<ProgressTrackingService> logger,
            IBackgroundJobClient backgroundJobClient)
        {
            _unitOfWork = unitOfWork;
            _scheduleRepository = scheduleRepository;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        #region Main System Status Check

        // <summary>
        // System-wide status check - focuses on Course Progress management
        // </summary>
        public async Task CheckAndUpdateAllStatuses()
        {
            try
            {
                _logger.LogInformation("Starting Course Progress status check");

                // Get all courses that need status updates
                var courses = await _unitOfWork.CourseRepository.FindAsync(
                    c => c.Status == CourseStatus.Approved &&
                         (c.Progress == Progress.NotYet || c.Progress == Progress.Ongoing));

                foreach (var course in courses)
                {
                    // 1. Update Course Progress: NotYet → Ongoing(when StartDate begins)
                    if (course.Progress == Progress.NotYet)
                    {
                        await UpdateCourseToOngoingAsync(course.CourseId);
                    }
                    // 2. Update Course Progress: Ongoing → Completed (when all conditions met)
                    else if (course.Progress == Progress.Ongoing)
                    {
                        await CheckAndUpdateCourseToCompletedAsync(course.CourseId);
                    }
                }

                _logger.LogInformation("Course Progress status check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Course Progress status check");
                throw;
            }
        }

        public async Task ScheduleCourseStatusUpdatesAsync()
        {
            try
            {
                _logger.LogInformation("Scheduling course status updates");

                var courses = await _unitOfWork.CourseRepository.FindAsync(
                    c => c.Status == CourseStatus.Approved &&
                         (c.Progress == Progress.NotYet || c.Progress == Progress.Ongoing));

                foreach (var course in courses)
                {
                    if (course.Progress == Progress.NotYet && course.StartDateTime > DateTime.Now)
                    {
                        // Schedule transition to Ongoing
                        _backgroundJobClient.Schedule(
                            () => UpdateCourseToOngoingAsync(course.CourseId),
                            course.StartDateTime);
                        _logger.LogInformation($"Scheduled course {course.CourseId} to Ongoing at {course.StartDateTime}");
                    }
                    else if (course.Progress == Progress.Ongoing)
                    {
                        // Schedule recurring check for completion
                        _backgroundJobClient.Schedule(
                            () => CheckAndUpdateCourseToCompletedAsync(course.CourseId),
                            TimeSpan.FromMinutes(5)); // Check every 30 minutes
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling course status updates");
                throw;
            }
        }

        /// <summary>
        /// Update courses from NotYet to Ongoing when StartDate begins
        /// </summary>
        public async Task UpdateCourseToOngoingAsync(string courseId)
        {
            try
            {
                var course = await _unitOfWork.CourseRepository.GetAsync(c => c.CourseId == courseId);
                if (course == null || course.Progress != Progress.NotYet || course.Status != CourseStatus.Approved)
                    return;

                course.Progress = Progress.Ongoing;
                course.UpdatedAt = DateTime.Now;
                await _unitOfWork.CourseRepository.UpdateAsync(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Course {courseId} updated to Ongoing");

                // Schedule completion check
                _backgroundJobClient.Schedule(
                    () => CheckAndUpdateCourseToCompletedAsync(courseId),
                    TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating course {courseId} to Ongoing");
                throw;
            }
        }

        /// <summary>
        /// Update courses from Ongoing to Completed when all conditions are met
        /// </summary>
        public async Task CheckAndUpdateCourseToCompletedAsync(string courseId)
        {
            try
            {
                var course = await _unitOfWork.CourseRepository.GetAsync(
                    c => c.CourseId == courseId, c => c.Classes);

                if (course == null || course.Progress != Progress.Ongoing)
                    return;

                if (await IsCourseCompleted(courseId))
                {
                    course.Progress = Progress.Completed;
                    course.UpdatedAt = DateTime.Now;
                    await _unitOfWork.CourseRepository.UpdateAsync(course);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Course {courseId} updated to Completed");
                }
                else
                {
                    // Reschedule completion check
                    _backgroundJobClient.Schedule(
                        () => CheckAndUpdateCourseToCompletedAsync(courseId),
                        TimeSpan.FromMinutes(5));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking course {courseId} for completion");
                throw;
            }
        }

        /// <summary>
        /// Check if a course is completed based on all conditions
        /// </summary>
        private async Task<bool> IsCourseCompleted(string courseId)
        {
            try
            {
                // Get course with classes
                var course = await _unitOfWork.CourseRepository.GetAsync(
                    c => c.CourseId == courseId, c => c.Classes);

                if (course?.Classes == null || !course.Classes.Any())
                    return false;

                // Check each class in the course
                foreach (var classEntity in course.Classes)
                {
                    var classSubjects = await _unitOfWork.ClassSubjectRepository
                        .FindAsync(cs => cs.ClassId == classEntity.ClassId);

                    foreach (var classSubject in classSubjects)
                    {
                        // Check if all schedules for this ClassSubject are completed
                        var schedules = await _scheduleRepository
                            .GetSchedulesByClassSubjectIdAsync(classSubject.ClassSubjectId);

                        if (schedules == null || !schedules.Any())
                            continue;

                        bool allSchedulesCompleted = schedules.All(s =>
                            s.Status == ScheduleStatus.Completed || s.Status == ScheduleStatus.Canceled);

                        if (!allSchedulesCompleted)
                            return false;

                        // Check if all trainees have grades
                        var traineeAssigns = await _unitOfWork.TraineeAssignRepository
                            .FindAsync(ta => ta.ClassSubjectId == classSubject.ClassSubjectId);

                        if (traineeAssigns.Any())
                        {
                            var grades = await _unitOfWork.GradeRepository
                                .FindAsync(g => traineeAssigns.Select(ta => ta.TraineeAssignId).Contains(g.TraineeAssignID));

                            bool allGraded = traineeAssigns.All(ta =>
                                grades.Any(g => g.TraineeAssignID == ta.TraineeAssignId));

                            if (!allGraded)
                                return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if course {courseId} is completed");
                return false;
            }
        }

        #endregion

        #region Existing Methods (Kept for backward compatibility)

        /// <summary>
        /// Checks and updates the progress status of a ClassSubject based on schedules and grades.
        /// </summary>
        public async Task CheckAndUpdateClassSubjectStatus(string classSubjectId)
        {
            try
            {
                _logger.LogInformation($"Checking status for ClassSubject ID: {classSubjectId}");

                var classSubject = (await _unitOfWork.ClassSubjectRepository
                    .FindIncludeAsync(cs => cs.ClassSubjectId == classSubjectId,
                                      cs => cs.Class, cs => cs.traineeAssigns))
                    .FirstOrDefault();

                if (classSubject == null)
                {
                    _logger.LogWarning($"ClassSubject with ID {classSubjectId} not found");
                    return;
                }

                if (classSubject?.traineeAssigns == null)
                {
                    _logger.LogWarning($"ClassSubject {classSubjectId} has no trainee assignments");
                    return;
                }

                var schedules = await _scheduleRepository
                    .GetSchedulesByClassSubjectIdAsync(classSubjectId);

                if (schedules == null || !schedules.Any())
                {
                    _logger.LogWarning($"No schedules found for ClassSubject {classSubjectId}");
                    return;
                }

                //bool allSchedulesEnded = schedules.All(s =>
                //    DateTime.Now > s.EndDateTime || s.Status == ScheduleStatus.Completed);

                //if (!allSchedulesEnded)
                //{
                //    _logger.LogInformation($"ClassSubject {classSubjectId} has schedules that haven't ended yet");
                //    return;
                //}

                var traineeAssigns = classSubject.traineeAssigns.ToList();
                var traineeAssignIds = traineeAssigns.Select(ta => ta.TraineeAssignId).ToList();
                var grades = await _unitOfWork.GradeRepository
                    .FindAsync(g => traineeAssignIds.Contains(g.TraineeAssignID));

                bool allTraineesGraded = traineeAssigns.All(ta =>
                    grades.Any(g => g.TraineeAssignID == ta.TraineeAssignId));

                if (!allTraineesGraded)
                {
                    _logger.LogInformation($"ClassSubject {classSubjectId} has trainees without grades");
                    return;
                }

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
                _logger.LogInformation($"All schedules for ClassSubject {classSubjectId} marked as Completed");

                // Check if course should be updated to Completed
                await CheckAndUpdateCourseStatus(classSubject.Class.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking status for ClassSubject {classSubjectId}");
                throw;
            }
        }

        /// <summary>
        /// Legacy method maintained for backward compatibility.
        /// </summary>
        public async Task CheckAndUpdateCourseSubjectSpecialtyStatus(string courseSubjectSpecialtyId)
        {
            _logger.LogWarning($"CheckAndUpdateCourseSubjectSpecialtyStatus is deprecated. Redirecting to CheckAndUpdateClassSubjectStatus");
            await CheckAndUpdateClassSubjectStatus(courseSubjectSpecialtyId);
        }

        /// <summary>
        /// Checks and updates the Course progress status.
        /// </summary>
        public async Task CheckAndUpdateCourseStatus(string courseId)
        {
            try
            {
                _logger.LogInformation($"Checking status for Course ID: {courseId}");

                var course = await _unitOfWork.CourseRepository
                    .GetAsync(c => c.CourseId == courseId, c => c.Classes);

                if (course == null)
                {
                    _logger.LogWarning($"Course with ID {courseId} not found");
                    return;
                }

                if (course.Progress == Progress.Completed || course.Progress == Progress.NotYet)
                {
                    _logger.LogInformation($"Course {courseId} is already {course.Progress}, no update needed");
                    return;
                }

                if (await IsCourseCompleted(courseId))
                {
                    course.Progress = Progress.Completed;
                    course.UpdatedAt = DateTime.Now;
                    await _unitOfWork.CourseRepository.UpdateAsync(course);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated Course {courseId} to Completed");
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
    }
}