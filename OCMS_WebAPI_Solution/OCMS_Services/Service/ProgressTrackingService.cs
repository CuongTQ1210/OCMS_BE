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

        #region Check and update Subject status
        /// <summary>
        /// Kiểm tra và cập nhật trạng thái của một Subject dựa trên lịch học và điểm số
        /// </summary>
        public async Task CheckAndUpdateSubjectStatus(string subjectId)
        {
            try
            {
                _logger.LogInformation($"Checking status for Subject ID: {subjectId}");

                var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);
                if (subject == null)
                {
                    _logger.LogWarning($"Subject with ID {subjectId} not found");
                    return;
                }

                // Lấy course để biết danh sách học viên
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(subject.CourseId);
                if (course == null)
                {
                    _logger.LogWarning($"Course with ID {subject.CourseId} not found for Subject {subjectId}");
                    return;
                }

                // Lấy tất cả lịch học của subject
                var schedules = await _scheduleRepository.GetSchedulesBySubjectIdAsync(subjectId);
                if (schedules == null || !schedules.Any())
                {
                    _logger.LogWarning($"No schedules found for Subject {subjectId}");
                    return;
                }

                // THÊM: Kiểm tra xem subject đã bắt đầu chưa
                bool hasStarted = schedules.Any(s => DateTime.Now >= s.StartDateTime);
                if (!hasStarted)
                {
                    _logger.LogInformation($"Subject {subjectId} has not started yet");
                    return;
                }

                // Kiểm tra tất cả lịch học đã kết thúc
                bool allSchedulesEnded = schedules.All(s =>
                    DateTime.Now > s.EndDateTime && s.Status != ScheduleStatus.Completed);

                if (!allSchedulesEnded)
                {
                    _logger.LogInformation($"Subject {subjectId} has schedules that haven't ended yet");
                    return;
                }

                // Kiểm tra tất cả học viên đã có điểm
                var traineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(ta => ta.CourseId == course.CourseId);
                if (traineeAssigns == null || !traineeAssigns.Any())
                {
                    _logger.LogWarning($"No trainees assigned to Course {course.CourseId}");
                    return;
                }

                var grades = await _unitOfWork.GradeRepository.FindAsync(g => g.SubjectId == subjectId);

                // Kiểm tra xem mỗi học viên được assign đã có điểm chưa
                bool allTraineesGraded = true;
                foreach (var traineeAssign in traineeAssigns)
                {
                    bool hasGrade = grades.Any(g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
                    if (!hasGrade)
                    {
                        allTraineesGraded = false;
                        _logger.LogInformation($"Trainee assign {traineeAssign.TraineeAssignId} doesn't have grade for Subject {subjectId}");
                        break;
                    }
                }

                if (!allTraineesGraded)
                {
                    _logger.LogInformation($"Subject {subjectId} has trainees without grades");
                    return;
                }

                // Cập nhật trạng thái Schedule thành Completed
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
                _logger.LogInformation($"All schedules for Subject {subjectId} marked as Completed");

                // Kiểm tra và cập nhật trạng thái Course
                await CheckAndUpdateCourseStatus(subject.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking status for Subject {subjectId}");
                throw;
            }
        }
        #endregion

        #region Check and update Course status
        /// <summary>
        /// Kiểm tra và cập nhật trạng thái Course dựa trên trạng thái của tất cả Subject
        /// </summary>
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

                // Đã completed hoặc không phải đang ongoing thì bỏ qua
                if (course.Progress == Progress.Completed || course.Progress == Progress.NotYet)
                {
                    _logger.LogInformation($"Course {courseId} is already {course.Progress}, no update needed");
                    return;
                }

                // Lấy tất cả subject của course
                var subjects = await _unitOfWork.SubjectRepository.FindAsync(s => s.CourseId == courseId);
                if (subjects == null || !subjects.Any())
                {
                    _logger.LogWarning($"No subjects found for Course {courseId}");
                    return;
                }

                // Kiểm tra tất cả subject đã có lịch học hoàn thành
                bool allSubjectsCompleted = true;
                foreach (var subject in subjects)
                {
                    var schedules = await _scheduleRepository.GetSchedulesBySubjectIdAsync(subject.SubjectId);

                    if (schedules == null || !schedules.Any())
                    {
                        allSubjectsCompleted = false;
                        _logger.LogInformation($"Subject {subject.SubjectId} has no schedules");
                        break;
                    }

                    bool subjectCompleted = schedules.All(s => s.Status == ScheduleStatus.Completed || s.Status == ScheduleStatus.Canceled);
                    if (!subjectCompleted)
                    {
                        allSubjectsCompleted = false;
                        _logger.LogInformation($"Subject {subject.SubjectId} has incomplete schedules");
                        break;
                    }

                    // Kiểm tra tất cả học viên đã có điểm
                    var traineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(ta => ta.CourseId == courseId);
                    var grades = await _unitOfWork.GradeRepository.FindAsync(g => g.SubjectId == subject.SubjectId);

                    foreach (var traineeAssign in traineeAssigns)
                    {
                        bool hasGrade = grades.Any(g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
                        if (!hasGrade)
                        {
                            allSubjectsCompleted = false;
                            _logger.LogInformation($"Trainee {traineeAssign.TraineeId} has no grade for Subject {subject.SubjectId}");
                            break;
                        }
                    }

                    if (!allSubjectsCompleted)
                        break;
                }

                if (allSubjectsCompleted)
                {
                    course.Progress = Progress.Completed;
                    course.UpdatedAt = DateTime.Now;
                    await _unitOfWork.CourseRepository.UpdateAsync(course);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated Course {courseId} to Completed");

                    // Kiểm tra và cập nhật trạng thái TrainingPlan
                    await CheckAndUpdateTrainingPlanStatus(course.TrainingPlanId);
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

        #region Check and update TrainingPlan status
        /// <summary>
        /// Kiểm tra và cập nhật trạng thái TrainingPlan dựa trên trạng thái của tất cả Course
        /// </summary>
        public async Task CheckAndUpdateTrainingPlanStatus(string planId)
        {
            try
            {
                _logger.LogInformation($"Checking status for Training Plan ID: {planId}");

                var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    _logger.LogWarning($"Training Plan with ID {planId} not found");
                    return;
                }

                // Đã completed hoặc không phải đang active/pending/approved thì bỏ qua
                if (plan.TrainingPlanStatus == TrainingPlanStatus.Completed ||
                    plan.TrainingPlanStatus == TrainingPlanStatus.Draft ||
                    plan.TrainingPlanStatus == TrainingPlanStatus.Rejected)
                {
                    _logger.LogInformation($"Training Plan {planId} is {plan.TrainingPlanStatus}, no update needed");
                    return;
                }

                // Lấy tất cả course của training plan
                var courses = await _unitOfWork.CourseRepository.FindAsync(c => c.TrainingPlanId == planId);
                if (courses == null || !courses.Any())
                {
                    _logger.LogWarning($"No courses found for Training Plan {planId}");
                    return;
                }

                // Kiểm tra tất cả course đã hoàn thành
                bool allCoursesCompleted = courses.All(c => c.Progress == Progress.Completed);

                if (allCoursesCompleted)
                {
                    plan.TrainingPlanStatus = TrainingPlanStatus.Completed;
                    plan.ModifyDate = DateTime.Now;
                    await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated Training Plan {planId} to Completed");
                }
                else
                {
                    _logger.LogInformation($"Training Plan {planId} has incomplete courses");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking status for Training Plan {planId}");
                throw;
            }
        }
        #endregion

        #region System-wide status check
        public async Task CheckAndUpdateAllStatuses()
        {
            try
            {
                _logger.LogInformation("Starting system-wide status check");

                // Kiểm tra các Schedule đã đến thời gian bắt đầu
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

                // 1. Kiểm tra các Schedule đã qua EndDateTime
                var expiredSchedules = await _unitOfWork.TrainingScheduleRepository.FindAsync(
                    s => s.Status == ScheduleStatus.Incoming && s.EndDateTime < DateTime.Now);

                var processedSubjectIds = new HashSet<string>();

                foreach (var schedule in expiredSchedules)
                {
                    // Chỉ xử lý mỗi subject một lần
                    if (!processedSubjectIds.Contains(schedule.SubjectID))
                    {
                        await CheckAndUpdateSubjectStatus(schedule.SubjectID);
                        processedSubjectIds.Add(schedule.SubjectID);
                    }
                }

                _logger.LogInformation($"Processed {processedSubjectIds.Count} subjects with expired schedules");

                // 2. Kiểm tra các Course đang Ongoing
                var ongoingCourses = await _unitOfWork.CourseRepository.FindAsync(
                    c => c.Progress == Progress.Ongoing && c.Status == CourseStatus.Approved);

                foreach (var course in ongoingCourses)
                {
                    await CheckAndUpdateCourseStatus(course.CourseId);
                }

                _logger.LogInformation($"Processed {ongoingCourses.Count()} ongoing courses");

                // 3. Kiểm tra các TrainingPlan đang active
                var activePlans = await _unitOfWork.TrainingPlanRepository.FindAsync(
                    p => p.TrainingPlanStatus == TrainingPlanStatus.Approved ||
                         p.TrainingPlanStatus == TrainingPlanStatus.Pending);

                foreach (var plan in activePlans)
                {
                    await CheckAndUpdateTrainingPlanStatus(plan.PlanId);
                }

                _logger.LogInformation($"Processed {activePlans.Count()} active training plans");
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
