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
/// CourseSubjectSpecialty, Course, and TrainingPlan.
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

        #region Check and update CourseSubjectSpecialty progress
        /// <summary>
        /// Checks and updates the progress status of a CourseSubjectSpecialty based on schedules and grades.
        /// A CourseSubjectSpecialty is considered completed when all schedules have ended and all trainees have grades.
        /// </summary>
        /// <param name="courseSubjectSpecialtyId">The ID of the CourseSubjectSpecialty to check</param>
        public async Task CheckAndUpdateCourseSubjectSpecialtyStatus(string courseSubjectSpecialtyId)
        {
            try
            {
                _logger.LogInformation($"Checking status for CourseSubjectSpecialty ID: {courseSubjectSpecialtyId}");

                var courseSubjectSpecialty = await _unitOfWork.CourseSubjectSpecialtyRepository.GetByIdAsync(courseSubjectSpecialtyId);
                if (courseSubjectSpecialty == null)
                {
                    _logger.LogWarning($"CourseSubjectSpecialty with ID {courseSubjectSpecialtyId} not found");
                    return;
                }

                // Get all schedules for this CourseSubjectSpecialty
                var schedules = await _scheduleRepository.GetSchedulesByCourseSubjectIdAsync(courseSubjectSpecialtyId);
                if (schedules == null || !schedules.Any())
                {
                    _logger.LogWarning($"No schedules found for CourseSubjectSpecialty {courseSubjectSpecialtyId}");
                    return;
                }

                // Check if the subject has started yet
                bool hasStarted = schedules.Any(s => DateTime.Now >= s.StartDateTime);
                if (!hasStarted)
                {
                    _logger.LogInformation($"CourseSubjectSpecialty {courseSubjectSpecialtyId} has not started yet");
                    return;
                }

                // Check if all schedules have ended
                bool allSchedulesEnded = schedules.All(s =>
                    DateTime.Now > s.EndDateTime && s.Status != ScheduleStatus.Completed);

                if (!allSchedulesEnded)
                {
                    _logger.LogInformation($"CourseSubjectSpecialty {courseSubjectSpecialtyId} has schedules that haven't ended yet");
                    return;
                }

                // Get all trainee assignments for this CourseSubjectSpecialty
                var traineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(ta =>
                    ta.CourseSubjectSpecialty.Id == courseSubjectSpecialtyId);

                if (traineeAssigns == null || !traineeAssigns.Any())
                {
                    _logger.LogWarning($"No trainees assigned to CourseSubjectSpecialty {courseSubjectSpecialtyId}");
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
                    await CheckAndUpdateCourseStatus(courseSubjectSpecialty.CourseId);
                    return;
                }

                // Get all grades for this CourseSubjectSpecialty
                var grades = await _unitOfWork.GradeRepository.FindAsync(g =>
                    g.TraineeAssign.CourseSubjectSpecialty.Id == courseSubjectSpecialtyId);

                // Check if all assigned trainees have grades
                bool allTraineesGraded = true;
                foreach (var traineeAssign in traineeAssigns)
                {
                    bool hasGrade = grades.Any(g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
                    if (!hasGrade)
                    {
                        allTraineesGraded = false;
                        _logger.LogInformation($"Trainee assign {traineeAssign.TraineeAssignId} doesn't have grade for CourseSubjectSpecialty {courseSubjectSpecialtyId}");
                        break;
                    }
                }

                if (!allTraineesGraded)
                {
                    _logger.LogInformation($"CourseSubjectSpecialty {courseSubjectSpecialtyId} has trainees without grades");
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
                _logger.LogInformation($"All schedules for CourseSubjectSpecialty {courseSubjectSpecialtyId} marked as Completed");

                // Check and update the related Course status
                await CheckAndUpdateCourseStatus(courseSubjectSpecialty.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking status for CourseSubjectSpecialty {courseSubjectSpecialtyId}");
                throw;
            }
        }
        #endregion

        #region Check and update Course progress
        /// <summary>
        /// Checks and updates the Course progress status based on all related CourseSubjectSpecialty completions.
        /// A Course is considered completed when all its CourseSubjectSpecialty combinations are completed.
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

                // Get all CourseSubjectSpecialty entries for this course
                var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository.FindAsync(css =>
                    css.CourseId == courseId);

                if (courseSubjectSpecialties == null || !courseSubjectSpecialties.Any())
                {
                    _logger.LogWarning($"No CourseSubjectSpecialties found for Course {courseId}");
                    return;
                }

                // Check if all CourseSubjectSpecialties are completed
                bool allCourseSubjectsCompleted = true;
                foreach (var courseSubjectSpecialty in courseSubjectSpecialties)
                {
                    // Get all schedules for this CourseSubjectSpecialty
                    var schedules = await _scheduleRepository.GetSchedulesByCourseSubjectIdAsync(courseSubjectSpecialty.Id);

                    if (schedules == null || !schedules.Any())
                    {
                        allCourseSubjectsCompleted = false;
                        _logger.LogInformation($"CourseSubjectSpecialty {courseSubjectSpecialty.Id} has no schedules");
                        break;
                    }

                    bool subjectCompleted = schedules.All(s =>
                        s.Status == ScheduleStatus.Completed || s.Status == ScheduleStatus.Canceled);

                    if (!subjectCompleted)
                    {
                        allCourseSubjectsCompleted = false;
                        _logger.LogInformation($"CourseSubjectSpecialty {courseSubjectSpecialty.Id} has incomplete schedules");
                        break;
                    }

                    // Check if all trainees have grades
                    var traineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(ta =>
                        ta.CourseSubjectSpecialty.Id == courseSubjectSpecialty.Id);

                    var grades = await _unitOfWork.GradeRepository.FindAsync(g =>
                        g.TraineeAssign.CourseSubjectSpecialty.Id == courseSubjectSpecialty.Id);

                    foreach (var traineeAssign in traineeAssigns)
                    {
                        bool hasGrade = grades.Any(g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
                        if (!hasGrade)
                        {
                            allCourseSubjectsCompleted = false;
                            _logger.LogInformation($"Trainee {traineeAssign.TraineeId} has no grade for CourseSubjectSpecialty {courseSubjectSpecialty.Id}");
                            break;
                        }
                    }

                    if (!allCourseSubjectsCompleted)
                        break;
                }

                if (allCourseSubjectsCompleted)
                {
                    course.Progress = Progress.Completed;
                    course.UpdatedAt = DateTime.Now;
                    await _unitOfWork.CourseRepository.UpdateAsync(course);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated Course {courseId} to Completed");

                    // Check and update all related TrainingPlans
                    /*
                    var trainingPlans = await _unitOfWork.TrainingPlanRepository.FindAsync(tp => tp.CourseId == courseId);
                    foreach (var trainingPlan in trainingPlans)
                    {
                        await CheckAndUpdateTrainingPlanStatus(trainingPlan.PlanId);
                    }
                    */
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
            /*
            try
            {
                _logger.LogInformation($"Checking status for Training Plan ID: {planId}");

                var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    _logger.LogWarning($"Training Plan with ID {planId} not found");
                    return;
                }

                // If already completed or in draft/rejected state, skip
                if (plan.TrainingPlanStatus == TrainingPlanStatus.Completed ||
                    plan.TrainingPlanStatus == TrainingPlanStatus.Draft ||
                    plan.TrainingPlanStatus == TrainingPlanStatus.Rejected)
                {
                    _logger.LogInformation($"Training Plan {planId} is {plan.TrainingPlanStatus}, no update needed");
                    return;
                }

                // Get the course associated with this training plan
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(plan.CourseId);
                if (course == null)
                {
                    _logger.LogWarning($"Course not found for Training Plan {planId}");
                    return;
                }

                // Check if the course is completed
                if (course.Progress == Progress.Completed)
                {
                    plan.TrainingPlanStatus = TrainingPlanStatus.Completed;
                    plan.ModifyDate = DateTime.Now;
                    await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated Training Plan {planId} to Completed");
                }
                else
                {
                    _logger.LogInformation($"Training Plan {planId} has incomplete course {course.CourseId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking status for Training Plan {planId}");
                throw;
            }
            */
        }
        #endregion

        #region System-wide progress check
        /// <summary>
        /// Performs a system-wide check and update of all educational entities' progress statuses.
        /// This method:
        /// 1. Updates schedule statuses (Approved → Incoming)
        /// 2. Processes completed CourseSubjectSpecialties
        /// 3. Updates Course progress (Ongoing → Completed)
        /// 4. Updates TrainingPlan statuses (Approved → Completed)
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

                var processedCourseSubjectIds = new HashSet<string>();

                foreach (var schedule in expiredSchedules)
                {
                    // Process each CourseSubjectSpecialty only once
                    if (!processedCourseSubjectIds.Contains(schedule.CourseSubjectSpecialty.Id))
                    {
                        await CheckAndUpdateCourseSubjectSpecialtyStatus(schedule.CourseSubjectSpecialty.Id);
                        processedCourseSubjectIds.Add(schedule.CourseSubjectSpecialty.Id);
                    }
                }

                _logger.LogInformation($"Processed {processedCourseSubjectIds.Count} CourseSubjectSpecialties with expired schedules");

                // 3. Check ongoing courses
                var ongoingCourses = await _unitOfWork.CourseRepository.FindAsync(
                    c => c.Progress == Progress.Ongoing && c.Status == CourseStatus.Approved);

                foreach (var course in ongoingCourses)
                {
                    await CheckAndUpdateCourseStatus(course.CourseId);
                }

                _logger.LogInformation($"Processed {ongoingCourses.Count()} ongoing courses");

                // 4. Check active training plans
                /*
                var activePlans = await _unitOfWork.TrainingPlanRepository.FindAsync(
                    p => p.TrainingPlanStatus == TrainingPlanStatus.Approved ||
                         p.TrainingPlanStatus == TrainingPlanStatus.Pending);

                foreach (var plan in activePlans)
                {
                    await CheckAndUpdateTrainingPlanStatus(plan.PlanId);
                }

                _logger.LogInformation($"Processed {activePlans.Count()} active training plans");
                */
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