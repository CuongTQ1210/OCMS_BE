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

                // Sử dụng GetAsync với includes thay vì GetByIdAsync với multiple parameters
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

                // Lấy tất cả schedules cho ClassSubject này
                var schedules = await _scheduleRepository
                    .GetSchedulesByClassSubjectIdAsync(classSubjectId);

                if (schedules == null || !schedules.Any())
                {
                    _logger.LogWarning($"No schedules found for ClassSubject {classSubjectId}");
                    return;
                }

                // Kiểm tra xem tất cả schedules đã kết thúc chưa
                bool allSchedulesEnded = schedules.All(s =>
                    DateTime.Now > s.EndDateTime || s.Status == ScheduleStatus.Completed);

                if (!allSchedulesEnded)
                {
                    _logger.LogInformation($"ClassSubject {classSubjectId} has schedules that haven't ended yet");
                    return;
                }

                // Lấy tất cả traineeAssigns và grades trong một truy vấn
                var traineeAssigns = classSubject.traineeAssigns.ToList();
                var traineeAssignIds = traineeAssigns.Select(ta => ta.TraineeAssignId).ToList();
                var grades = await _unitOfWork.GradeRepository
                    .FindAsync(g => traineeAssignIds.Contains(g.TraineeAssignID));

                // Kiểm tra xem tất cả traineeAssigns đều có grades
                bool allTraineesGraded = traineeAssigns.All(ta =>
                    grades.Any(g => g.TraineeAssignID == ta.TraineeAssignId));

                if (!allTraineesGraded)
                {
                    _logger.LogInformation($"ClassSubject {classSubjectId} has trainees without grades");
                    return;
                }

                // Cập nhật tất cả schedules thành Completed
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

                // Kiểm tra và cập nhật Course status
                await CheckAndUpdateCourseStatus(classSubject.Class.CourseId);
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

                // Sử dụng GetAsync thay vì GetByIdAsync với includes
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

                // Lấy tất cả class ids để lấy class subjects
                var classIds = course.Classes.Select(c => c.ClassId).ToList();
                var classSubjects = await _unitOfWork.ClassSubjectRepository
                    .FindAsync(cs => classIds.Contains(cs.ClassId));

                if (!classSubjects.Any())
                {
                    _logger.LogWarning($"No ClassSubjects found for Course {courseId}");
                    return;
                }

                // Kiểm tra xem tất cả classSubjects đã hoàn thành chưa
                bool allClassSubjectsCompleted = true;
                foreach (var classSubject in classSubjects)
                {
                    var schedules = await _scheduleRepository
                        .GetSchedulesByClassSubjectIdAsync(classSubject.ClassSubjectId);

                    bool subjectCompleted = schedules.All(s =>
                        s.Status == ScheduleStatus.Completed || s.Status == ScheduleStatus.Canceled);

                    if (!subjectCompleted)
                    {
                        allClassSubjectsCompleted = false;
                        break;
                    }

                    // Kiểm tra grades cho traineeAssigns
                    var traineeAssigns = await _unitOfWork.TraineeAssignRepository
                        .FindAsync(ta => ta.ClassSubjectId == classSubject.ClassSubjectId);

                    var grades = await _unitOfWork.GradeRepository
                        .FindAsync(g => traineeAssigns.Select(ta => ta.TraineeAssignId).Contains(g.TraineeAssignID));

                    bool allGraded = traineeAssigns.All(ta =>
                        grades.Any(g => g.TraineeAssignID == ta.TraineeAssignId));

                    if (!allGraded)
                    {
                        allClassSubjectsCompleted = false;
                        break;
                    }
                }

                if (allClassSubjectsCompleted)
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
