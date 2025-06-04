using AutoMapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Repositories.Repository;
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
    public class RequestService : IRequestService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;
        private readonly Lazy<ITrainingScheduleService> _trainingScheduleService;
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorAssignmentRepository _instructorAssignmentRepository;
        private readonly ITraineeAssignRepository _traineeAssignRepository;
        private readonly IGradeService _gradeService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public RequestService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            Lazy<ITrainingScheduleService> trainingScheduleService,
            ITrainingScheduleRepository trainingScheduleRepository,
            ICourseRepository courseRepository,
            IInstructorAssignmentRepository instructorAssignmentRepository,
            ITraineeAssignRepository traineeAssignRepository,
            IGradeService gradeService,
            IBackgroundJobClient backgroundJobClient)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
            _trainingScheduleRepository = trainingScheduleRepository ?? throw new ArgumentNullException(nameof(trainingScheduleRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _instructorAssignmentRepository = instructorAssignmentRepository ?? throw new ArgumentNullException(nameof(instructorAssignmentRepository));
            _traineeAssignRepository = traineeAssignRepository ?? throw new ArgumentNullException(nameof(traineeAssignRepository));
            _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
        }
        public RequestService(UnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IUserRepository userRepository, ICandidateRepository candidateRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
        }

        #region Create Request
        public async Task<Request> CreateRequestAsync(RequestDTO requestDto, string userId)
        {
            if (requestDto == null)
                throw new ArgumentNullException(nameof(requestDto));
            if (!Enum.IsDefined(typeof(RequestType), requestDto.RequestType))
                throw new ArgumentException($"Invalid RequestType: {requestDto.RequestType}");
            User user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            bool isValidEntity = await ValidateRequestEntityIdAsync(requestDto.RequestType, requestDto.RequestEntityId);
            if (!isValidEntity)
                throw new ArgumentException("Invalid RequestEntityId for the given RequestType.");

            var newRequest = new Request
            {
                RequestId = GenerateRequestId(),
                RequestUserId = userId,
                RequestUser = user,
                RequestEntityId = requestDto.RequestEntityId,
                Status = RequestStatus.Pending,
                RequestType = requestDto.RequestType,
                Description = requestDto.Description,
                Notes = requestDto.Notes,
                RequestDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ApproveByUserId = null,
                ApprovedDate = null
            };

            await _unitOfWork.RequestRepository.AddAsync(newRequest);
            await _unitOfWork.SaveChangesAsync();

            // Send notification to the director if NewPlan, RecurrentPlan, RelearnPlan
            if (newRequest.RequestType == RequestType.NewPlan ||
                newRequest.RequestType == RequestType.Update ||
                newRequest.RequestType == RequestType.Delete ||
                newRequest.RequestType == RequestType.AssignTrainee ||
                newRequest.RequestType == RequestType.AddTraineeAssign ||
                newRequest.RequestType == RequestType.SignRequest||
                newRequest.RequestType == RequestType.AssignInstructor||
                newRequest.RequestType == RequestType.ClassSchedule
                )
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "Request"
                    ));
                }
            }
            if (newRequest.RequestType == RequestType.SignRequest)

            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} for certificateId {requestDto.RequestEntityId} need to be signed.",
                        "Request"
                    ));
                }
            }
            else if (newRequest.RequestType == RequestType.CreateNew ||
                newRequest.RequestType == RequestType.CreateRecurrent ||
                newRequest.RequestType == RequestType.CreateRelearn)
            {

                var eduofficers = await _userRepository.GetUsersByRoleAsync("Training staff");

                foreach (var edu in eduofficers)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        edu.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "Request"
                    ));
                }
            }

            if (newRequest.RequestType == RequestType.CandidateImport)
            {
                var staffs = await _userRepository.GetUsersByRoleAsync("Training staff");
                foreach (var staff in staffs)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        staff.UserId,
                        "New Candidate Import Request",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "CandidateImport"
                    ));
                }
            }

            if (newRequest.RequestType == RequestType.DecisionTemplate)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Template Approval Request",
                        "A new template approval request has been submitted for review.",
                        "TemplateApprove"
                    ));
                }
            }
            if (newRequest.RequestType == RequestType.CertificateTemplate)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Template Approval Request",
                        "A new template approval request has been submitted for review.",
                        "TemplateApprove"
                    ));
                }
            }
            if (newRequest.RequestType == RequestType.Revoke)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        director.UserId,
                        "Revoke Certificate Approval Request",
                        "A new Revoke approval request has been submitted for review.",
                        "RevokeCertificate"
                    ));
                }
            }
            if (newRequest.RequestType == RequestType.NewCourse ||
                newRequest.RequestType == RequestType.UpdateCourse ||
                newRequest.RequestType == RequestType.DeleteCourse)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Course Request Submitted",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "CourseRequest"
                    ));
                }
            }
            return newRequest;
        }
        #endregion  

        #region Get All Requests
        public async Task<IEnumerable<RequestModel>> GetAllRequestsAsync()
        {
            var requests = await _unitOfWork.RequestRepository.GetAllAsync(); // Remove includeProperties


            return _mapper.Map<IEnumerable<RequestModel>>(requests);
        }
        #endregion

        #region Get Request By Id
        public async Task<RequestModel> GetRequestByIdAsync(string requestId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId); // Remove includeProperties

            return _mapper.Map<RequestModel>(request);
        }
        #endregion

        #region Get Requests for HeadMaster
        public async Task<List<RequestModel>> GetRequestsForHeadMasterAsync()
        {
            var validRequestTypes = new[]
            {
                RequestType.NewPlan,
                RequestType.ClassSchedule,
                RequestType.AssignInstructor,
                RequestType.AddTraineeAssign,
                RequestType.AssignTrainee,
                RequestType.DecisionTemplate,
                RequestType.CertificateTemplate,
                RequestType.Update,
                RequestType.Delete,
                RequestType.SignRequest,
                RequestType.NewCourse,
                RequestType.UpdateCourse,
                RequestType.DeleteCourse
            };

            var requests = await _unitOfWork.RequestRepository.GetAllAsync(
                predicate: r => validRequestTypes.Contains(r.RequestType));

            return _mapper.Map<List<RequestModel>>(requests);
        }
        #endregion

        #region Get Requests for Training Staff
        public async Task<List<RequestModel>> GetRequestsForEducationOfficerAsync()
        {
            var validRequestTypes = new[]
            {
                RequestType.CreateNew,
                RequestType.CreateRecurrent,
                RequestType.CreateRelearn,
                RequestType.Complaint,
                RequestType.CandidateImport,
                RequestType.AssignTrainee,
                RequestType.AddTraineeAssign,
                RequestType.Revoke
            };

            var requests = await _unitOfWork.RequestRepository.GetAllAsync(
                predicate: r => validRequestTypes.Contains(r.RequestType));

            return _mapper.Map<List<RequestModel>>(requests);
        }
        #endregion

        #region Helper Methods
        private string GenerateRequestId()
        {
            return $"REQ-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        private async Task<bool> ValidateRequestEntityIdAsync(RequestType requestType, string entityId)
        {
            switch (requestType)
            {
                case RequestType.NewPlan:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;

                    // Check if entity exists in the RequestRepository
                    var planExists = await _unitOfWork.RequestRepository.ExistsAsync(r => r.RequestEntityId == entityId);
                    if (!planExists)
                        throw new KeyNotFoundException("Training plan not found.");

                    // Check course status
                    var course = await _courseRepository.GetCourseWithDetailsAsync(entityId);
                    if (course == null)
                        throw new InvalidOperationException("Training plan must have at least one course.");

                    if (course.Status != CourseStatus.Approved)
                        throw new InvalidOperationException("Course have to be Approve.");

                    // Check for subject specialties
                    var hasSubjectSpecialties = (course.SubjectSpecialties?.Any() == true);
                    if (!hasSubjectSpecialties)
                        throw new InvalidOperationException($"Course '{course.CourseName}' must have at least one subject for specialty.");

                    return true;

                case RequestType.Update:
                case RequestType.Delete:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;

                    // Check for "CourseId:...specialtyId:..." format
                    if (entityId.StartsWith("CourseId:") && entityId.Contains("specialtyId:"))
                    {
                        // Parse the format
                        string fullId = entityId;
                        int specialtyIdIndex = fullId.IndexOf("specialtyId:");
                        string courseIdPart = fullId.Substring(9, specialtyIdIndex - 9);
                        string specialtyIdPart = fullId.Substring(specialtyIdIndex + 12);

                        // Check if course and specialty exist
                        bool courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == courseIdPart);
                        bool specialtyExists = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyIdPart);

                        return courseExists && specialtyExists;
                    }

                    // Check "{CourseId}:{SpecialtyId}" format
                    if (entityId.Contains(":") && entityId.Split(':').Length == 2)
                    {
                        string[] parts = entityId.Split(':');
                        string courseId = parts[0];
                        string specialtyId = parts[1];

                        bool courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == courseId);
                        bool specialtyExists = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyId);

                        // Check if there are related subjects instead of CourseSubjectSpecialty
                        var courseData = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                        bool hasSubjects = courseData != null && courseData.SubjectSpecialties != null &&
                            courseData.SubjectSpecialties.Any(ss => ss.SpecialtyId == specialtyId);

                        if (!hasSubjects)
                            throw new InvalidOperationException($"No subjects found for Course: {courseId} - Specialty: {specialtyId}");

                        return courseExists && specialtyExists;
                    }

                    // Check "{CourseId}:{SubjectId}:{SpecialtyId}" format
                    if (entityId.Contains(":") && entityId.Split(':').Length == 3)
                    {
                        string[] parts = entityId.Split(':');
                        string courseId = parts[0];
                        string subjectId = parts[1];
                        string specialtyId = parts[2];

                        bool courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == courseId);
                        bool subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == subjectId);
                        bool specialtyExists = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyId);

                        // Check if this combination exists using SubjectSpecialty
                        var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                            ss => ss.SubjectId == subjectId && ss.SpecialtyId == specialtyId &&
                                 ss.Courses.Any(c => c.CourseId == courseId));

                        if (subjectSpecialty != null)
                            throw new InvalidOperationException($"Course-Subject-Specialty combination already exists.");

                        return courseExists && subjectExists && specialtyExists;
                    }

                    return true;

                case RequestType.CreateNew:
                case RequestType.CreateRecurrent:
                case RequestType.CreateRelearn:
                    return string.IsNullOrEmpty(entityId);

                case RequestType.Complaint:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;

                    return await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == entityId);

                case RequestType.DecisionTemplate:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    return await _unitOfWork.DecisionTemplateRepository.ExistsAsync(dt => dt.DecisionTemplateId == entityId);

                case RequestType.CertificateTemplate:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    return await _unitOfWork.CertificateTemplateRepository.ExistsAsync(dt => dt.CertificateTemplateId == entityId);

                case RequestType.SignRequest:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    return await _unitOfWork.CertificateRepository.ExistsAsync(dt => dt.CertificateId == entityId);

                case RequestType.Revoke:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    return await _unitOfWork.CertificateRepository.ExistsAsync(dt => dt.CertificateId == entityId);

                case RequestType.NewCourse:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;

                    var courseDetail = await _courseRepository.GetCourseWithDetailsAsync(entityId);
                    if (courseDetail == null)
                        throw new KeyNotFoundException("Course not found.");

                   
                    foreach (var subjectSpecialty in courseDetail.SubjectSpecialties)
                    {
                        // Verify subject exists
                        var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectSpecialty.SubjectId);
                        if (subject == null)
                            throw new InvalidOperationException($"Subject not found for ID '{subjectSpecialty.SubjectId}' in course '{courseDetail.CourseName}'");
                    }

                    return true;

                case RequestType.UpdateCourse:
                case RequestType.DeleteCourse:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;

                    var existingCourse = await _unitOfWork.CourseRepository.GetByIdAsync(entityId);
                    if (existingCourse == null)
                        throw new KeyNotFoundException("Course not found.");

                    if (existingCourse.Status != CourseStatus.Approved)
                        throw new InvalidOperationException("Only approved courses can be updated/deleted.");

                    return true;

                default:
                    return true;
            }
        }
        #endregion

        #region Delete Request
        public async Task<bool> DeleteRequestAsync(string requestId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return false;

            await _unitOfWork.RequestRepository.DeleteAsync(requestId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Approve Request
        public async Task<bool> ApproveRequestAsync(string requestId, string approvedByUserId)
        {
            try
            {
                var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
                if (request == null || request.Status != RequestStatus.Pending)
                    return false;

                if (request.Status == RequestStatus.Rejected)
                {
                    throw new InvalidOperationException("The request has already been rejected and cannot be approved.");
                }

                var approver = await _userRepository.GetByIdAsync(approvedByUserId);

                // Save basic information but don't change status yet
                request.ApproveByUserId = approvedByUserId;
                request.ApprovedDate = DateTime.Now;
                request.UpdatedAt = DateTime.Now;

                // Variable to check if all actions completed successfully
                bool actionSuccessful = false;

                // Handle request type-specific actions
                switch (request.RequestType)
                {
                    case RequestType.NewPlan:
                        // Plan approval logic simplified to match available repositories
                        actionSuccessful = true;
                        break;

                    case RequestType.Update:
                    case RequestType.Delete:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }

                        // Handle the request to delete all subjects for a course and specialty
                        if (request.RequestEntityId.StartsWith("CourseId:") && request.RequestEntityId.Contains("specialtyId:"))
                        {
                            string fullId = request.RequestEntityId;
                            int specialtyIdIndex = fullId.IndexOf("specialtyId:");
                            string courseIdPart = fullId.Substring(9, specialtyIdIndex - 9);
                            string specialtyIdPart = fullId.Substring(specialtyIdIndex + 12);

                            // Get course and update its relationships
                            var courseDetails = await _courseRepository.GetCourseWithDetailsAsync(courseIdPart);
                            if (courseDetails == null)
                                throw new KeyNotFoundException($"Course: {courseIdPart} not found");

                            // Remove subject specialty entries
                            var subjectSpecialties = courseDetails.SubjectSpecialties?
                                .Where(ss => ss.SpecialtyId == specialtyIdPart)
                                .ToList();

                            if (subjectSpecialties == null || !subjectSpecialties.Any())
                                throw new KeyNotFoundException($"No subjects found for Course: {courseIdPart} - Specialty: {specialtyIdPart}");

                            // Delete schedules associated with these specialties
                            foreach (var ss in subjectSpecialties)
                            {
                                // Find class subjects for this subject specialty
                                var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                                    cs => cs.SubjectSpecialty.SubjectId == ss.SubjectId);

                                foreach (var classSubject in classSubjects)
                                {
                                    // Delete schedules for this class subject
                                    var schedules = await _trainingScheduleRepository.GetSchedulesByClassSubjectIdAsync(classSubject.ClassSubjectId);
                                    foreach (var schedule in schedules)
                                    {
                                        await _trainingScheduleService.Value.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                                    }
                                }
                            }

                            await _unitOfWork.SaveChangesAsync();
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Delete Request Approved",
                                $"Your request to delete all subjects for Course: {courseIdPart} - Specialty: {specialtyIdPart} has been approved.",
                                "Delete Subjects"
                            ));

                            actionSuccessful = true;
                            break;
                        }

                        // Handle format "{CourseId}:{SpecialtyId}"
                        if (request.RequestEntityId.Contains(":") && request.RequestEntityId.Split(':').Length == 2)
                        {
                            string[] parts = request.RequestEntityId.Split(':');
                            string courseId = parts[0];
                            string specialtyId = parts[1];

                            // Get course and update relationships
                            var courseEntity = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                            if (courseEntity == null)
                                throw new KeyNotFoundException($"Course: {courseId} not found");

                            // Handle subject specialty removal
                            var subjectSpecialties = courseEntity.SubjectSpecialties?
                                .Where(ss => ss.SpecialtyId == specialtyId)
                                .ToList();

                            if (subjectSpecialties == null || !subjectSpecialties.Any())
                                throw new KeyNotFoundException($"No subjects found for Course: {courseId} - Specialty: {specialtyId}");

                            // Delete schedules
                            foreach (var ss in subjectSpecialties)
                            {
                                // Find class subjects for this subject specialty
                                var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                                    cs => cs.SubjectSpecialty.SubjectId == ss.SubjectId);

                                foreach (var classSubject in classSubjects)
                                {
                                    // Delete schedules for this class subject
                                    var schedules = await _trainingScheduleRepository.GetSchedulesByClassSubjectIdAsync(classSubject.ClassSubjectId);
                                    foreach (var schedule in schedules)
                                    {
                                        await _trainingScheduleService.Value.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                                    }
                                }
                            }

                            await _unitOfWork.SaveChangesAsync();
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Delete Request Approved",
                                $"Your request to delete all subjects for Course: {courseId} - Specialty: {specialtyId} has been approved.",
                                "Delete Subjects"
                            ));

                            actionSuccessful = true;
                            break;
                        }

                        // Handle format "{CourseId}:{SubjectId}:{SpecialtyId}" for adding subject
                        if (request.RequestType == RequestType.Update && request.RequestEntityId.Contains(":") && request.RequestEntityId.Split(':').Length == 3)
                        {
                            string[] parts = request.RequestEntityId.Split(':');
                            string courseId = parts[0];
                            string subjectId = parts[1];
                            string specialtyId = parts[2];

                            // Check if entities exist
                            var courseData = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
                            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);
                            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(specialtyId);

                            if (courseData == null)
                                throw new KeyNotFoundException($"Course with ID '{courseId}' not found.");
                            if (subject == null)
                                throw new KeyNotFoundException($"Subject with ID '{subjectId}' not found.");
                            if (specialty == null)
                                throw new KeyNotFoundException($"Specialty with ID '{specialtyId}' not found.");

                            // Check if combination already exists using SubjectSpecialty
                            var existingSubjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                                ss => ss.SubjectId == subjectId && ss.SpecialtyId == specialtyId &&
                                     ss.Courses.Any(c => c.CourseId == courseId));

                            if (existingSubjectSpecialty != null)
                                throw new InvalidOperationException($"Course-Subject-Specialty combination already exists");

                            // Create subject-specialty if doesn't exist
                            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                                ss => ss.SubjectId == subjectId && ss.SpecialtyId == specialtyId);

                            if (subjectSpecialty == null)
                            {
                                // Create new SubjectSpecialty
                                var newSS = new SubjectSpecialty
                                {
                                    SubjectSpecialtyId = Guid.NewGuid().ToString(),
                                    SubjectId = subjectId,
                                    SpecialtyId = specialtyId,
                                };

                                await _unitOfWork.SubjectSpecialtyRepository.AddAsync(newSS);
                                await _unitOfWork.SaveChangesAsync();

                                // Link to course
                                var updatedCourse = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                                updatedCourse.SubjectSpecialties.Add(newSS);
                                await _unitOfWork.CourseRepository.UpdateAsync(updatedCourse);
                            }
                            else
                            {
                                // Add existing SubjectSpecialty to course
                                var updatedCourse = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                                updatedCourse.SubjectSpecialties.Add(subjectSpecialty);
                                await _unitOfWork.CourseRepository.UpdateAsync(updatedCourse);
                            }

                            await _unitOfWork.SaveChangesAsync();
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Add Subject Request Approved",
                                $"Your request to add subject {subjectId} to course {courseId} with specialty {specialtyId} has been approved.",
                                "Add Subject"
                            ));

                            actionSuccessful = true;
                            break;
                        }

                        // Handle other updates/deletes
                        var requestEntity = await _unitOfWork.RequestRepository.GetByIdAsync(request.RequestEntityId);
                        if (requestEntity != null)
                        {
                            // Update status to pending
                            // Note: Since TrainingPlanRepository is no longer available,
                            // we're treating this request as a generic entity update
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Request Status Updated",
                                $"Your request for {request.RequestEntityId} has been set to pending for {request.RequestType.ToString().ToLower()}.",
                                $"{request.RequestType.ToString()}"
                            ));

                            actionSuccessful = true;
                        }
                        break;

                    case RequestType.AssignTrainee:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }
                        {
                            var traineeAssigns = await _unitOfWork.TraineeAssignRepository
                                .GetAllAsync(ta => ta.RequestId == request.RequestId);

                            if (traineeAssigns == null || !traineeAssigns.Any())
                                throw new Exception($"No TraineeAssigns found for RequestId {request.RequestId}.");

                            foreach (var traineeAssign in traineeAssigns)
                            {
                                if (traineeAssign.RequestStatus != RequestStatus.Pending)
                                    throw new Exception("One or more TraineeAssigns are not in a pending state.");

                                traineeAssign.RequestStatus = RequestStatus.Approved;
                                traineeAssign.ApproveByUserId = approvedByUserId;
                                traineeAssign.ApprovalDate = DateTime.UtcNow;

                                await _unitOfWork.TraineeAssignRepository.UpdateAsync(traineeAssign);

                                // Create new grade for the approved trainee assignment
                                var gradeDto = new GradeDTO
                                {
                                    TraineeAssignID = traineeAssign.TraineeAssignId,
                                    ParticipantScore = -1,
                                    AssignmentScore = -1,
                                    FinalExamScore = -1,
                                    FinalResitScore = -1,
                                    Remarks = "Grade initialized after assignment approval"
                                };

                                await _gradeService.CreateAsync(gradeDto, approvedByUserId);
                            }

                            actionSuccessful = true;
                            break;
                        }

                    case RequestType.AddTraineeAssign:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }
                        {
                            var traineeAssign = await _unitOfWork.TraineeAssignRepository
                                .FirstOrDefaultAsync(ta => ta.RequestId == request.RequestId);

                            if (traineeAssign == null)
                                throw new Exception($"TraineeAssign linked to RequestId {request.RequestId} not found.");

                            if (traineeAssign.RequestStatus != RequestStatus.Pending)
                                throw new Exception("TraineeAssign is not in a pending state.");

                            traineeAssign.RequestStatus = RequestStatus.Approved;
                            traineeAssign.ApproveByUserId = approvedByUserId;
                            traineeAssign.ApprovalDate = DateTime.UtcNow;
                            await _unitOfWork.TraineeAssignRepository.UpdateAsync(traineeAssign);

                            // Create new grade for the approved trainee assignment
                            var gradeDto = new GradeDTO
                            {
                                TraineeAssignID = traineeAssign.TraineeAssignId,
                                ParticipantScore = -1,
                                AssignmentScore = -1,
                                FinalExamScore = -1,
                                FinalResitScore = -1,
                                Remarks = "Grade initialized after assignment approval"
                            };

                            await _gradeService.CreateAsync(gradeDto, approvedByUserId);

                            actionSuccessful = true;
                            break;
                        }

                    case RequestType.CandidateImport:
                        if (approver == null || approver.RoleId != 3)
                        {
                            throw new UnauthorizedAccessException("Only Training Staff can approve candidate import requests.");
                        }
                        var candidates = await _candidateRepository.GetCandidatesByImportRequestIdAsync(requestId);
                        if (candidates != null && candidates.Any())
                        {
                            foreach (var candidate in candidates)
                            {
                                candidate.CandidateStatus = CandidateStatus.Approved;
                                await _unitOfWork.CandidateRepository.UpdateAsync(candidate);
                            }
                        }

                        var admins = await _userRepository.GetUsersByRoleAsync("Admin");
                        foreach (var admin in admins)
                        {
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                admin.UserId,
                                "Candidate Import Approved",
                                "The candidate import request has been approved. Please create user accounts for the new candidates.",
                                "CandidateImport"
                            ));
                        }
                        actionSuccessful = true;
                        break;

                    case RequestType.ClassSchedule:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }

                        var classSchedule = await _unitOfWork.TrainingScheduleRepository.GetByIdAsync(request.RequestEntityId);
                        if (classSchedule != null)
                        {
                            classSchedule.Status = ScheduleStatus.Incoming;
                            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(classSchedule);

                            // Send notification to the requester
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Schedule Approved",
                                $"Your schedule request has been approved. Schedule ID: {classSchedule.ScheduleID}",
                                "Schedule"
                            ));
                            var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(classSchedule.ClassSubjectId);
                            var instructor = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(classSubject.InstructorAssignmentID);
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                instructor.InstructorId,
                                "Class Schedule",
                                $"You have a new Schedule for class {classSchedule.ClassSubjectId}",
                                "Schedule"
                            ));
                            
                        }
                        

                        actionSuccessful = true;
                        break;

                    case RequestType.DecisionTemplate:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }
                        var template = await _unitOfWork.DecisionTemplateRepository.GetByIdAsync(request.RequestEntityId);
                        if (template != null)
                        {
                            template.TemplateStatus = (int)TemplateStatus.Active;
                            await _unitOfWork.DecisionTemplateRepository.UpdateAsync(template);
                        }
                        actionSuccessful = true;
                        break;

                    case RequestType.CertificateTemplate:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }
                        var certificateTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(request.RequestEntityId);
                        if (certificateTemplate != null)
                        {
                            certificateTemplate.templateStatus = TemplateStatus.Active;
                            await _unitOfWork.CertificateTemplateRepository.UpdateAsync(certificateTemplate);
                        }
                        actionSuccessful = true;
                        break;

                    case RequestType.Revoke:
                        if (approver == null || approver.RoleId != 3)
                        {
                            throw new UnauthorizedAccessException("Only TrainingStaff can approve this request.");
                        }
                        var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(request.RequestEntityId);
                        if (certificate != null)
                        {
                            certificate.Status = CertificateStatus.Revoked;
                            certificate.RevocationDate = DateTime.Now;
                            certificate.RevocationReason = request.Notes;
                            await _unitOfWork.CertificateRepository.UpdateAsync(certificate);
                        }
                        actionSuccessful = true;
                        break;

                    case RequestType.NewCourse:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }

                        var course = await _courseRepository.GetWithIncludesAsync(
                               c => c.CourseId == request.RequestEntityId,
                               query => query.Include(c => c.SubjectSpecialties)
                                    .ThenInclude(ss => ss.Subject)
                                    .Include(c => c.SubjectSpecialties)
                                    .ThenInclude(ss => ss.Specialty)
                                    .Include(c => c.CreatedByUser)
                                    .Include(c => c.RelatedCourse)
                           );
                        // Check if course has subject specialties
                        if (course.SubjectSpecialties == null || !course.SubjectSpecialties.Any())
                            throw new InvalidOperationException($"Course '{course.CourseName}' must have at least one subject.");

                        if (course != null)
                        {
                            course.Status = CourseStatus.Approved;
                            course.UpdatedAt = DateTime.Now; // Use UpdatedAt instead of ApprovalDate

                            await _unitOfWork.CourseRepository.UpdateAsync(course);

                            // Send notification
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Course Approved",
                                $"Your course '{course.CourseName}' has been approved.",
                                "Course"
                            ));
                        }
                        actionSuccessful = true;
                        break;

                    case RequestType.UpdateCourse:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }

                        var courseToUpdate = await _unitOfWork.CourseRepository.GetByIdAsync(request.RequestEntityId);
                        if (courseToUpdate != null)
                        {
                            courseToUpdate.Status = CourseStatus.Pending;
                            courseToUpdate.UpdatedAt = DateTime.Now;

                            await _unitOfWork.CourseRepository.UpdateAsync(courseToUpdate);

                            // Send notification
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Course Update Approved",
                                $"Your request to update course '{courseToUpdate.CourseName}' has been approved.",
                                "Course"
                            ));
                        }
                        actionSuccessful = true;
                        break;

                    case RequestType.DeleteCourse:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }

                        var courseToDelete = await _unitOfWork.CourseRepository.GetByIdAsync(request.RequestEntityId);
                        if (courseToDelete != null)
                        {
                            // Check if course is being used in any active plan
                            bool isUsedInPlan = await _unitOfWork.RequestRepository.ExistsAsync(
                                r => r.RequestEntityId == courseToDelete.CourseId &&
                                     r.RequestType == RequestType.NewPlan &&
                                     r.Status == RequestStatus.Approved);

                            if (isUsedInPlan)
                            {
                                throw new InvalidOperationException("Cannot delete course that is being used in an approved training plan.");
                            }

                            // Delete related class subjects
                            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                                cs => cs.ClassId == courseToDelete.CourseId);

                            foreach (var classSubject in classSubjects)
                            {
                                // Delete schedules
                                var schedules = await _trainingScheduleRepository.GetSchedulesByClassSubjectIdAsync(classSubject.ClassSubjectId);
                                foreach (var schedule in schedules)
                                {
                                    await _trainingScheduleService.Value.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                                }

                                // Delete trainee assignments
                                var traineeAssigns = await _traineeAssignRepository.GetTraineeAssignsByClassSubjectIdAsync(classSubject.ClassSubjectId);
                                foreach (var traineeAssign in traineeAssigns)
                                {
                                    await _unitOfWork.TraineeAssignRepository.DeleteAsync(traineeAssign.TraineeAssignId);
                                }

                                // Delete instructor assignments
                                var instructorAssignments = await _unitOfWork.InstructorAssignmentRepository.GetAllAsync(
                                    ia => ia.SubjectId == classSubject.SubjectSpecialty.SubjectId);

                                foreach (var ia in instructorAssignments)
                                {
                                    await _unitOfWork.InstructorAssignmentRepository.DeleteAsync(ia.AssignmentId);
                                }

                                // Delete class subject
                                await _unitOfWork.ClassSubjectRepository.DeleteAsync(classSubject.ClassSubjectId);
                            }

                            // Update course status
                            courseToDelete.Status = CourseStatus.Pending;
                            await _unitOfWork.CourseRepository.UpdateAsync(courseToDelete);

                            // Send notification
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Course Deletion Approved",
                                $"Your request to delete course '{courseToDelete.CourseName}' has been approved.",
                                "Course"
                            ));
                        }
                        actionSuccessful = true;
                        break;

                    case RequestType.AssignInstructor:
                        if (approver == null || approver.RoleId != 2)
                        {
                            throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                        }

                        var instructorAssignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(request.RequestEntityId);
                        if (instructorAssignment != null)
                        {
                            instructorAssignment.RequestStatus = RequestStatus.Approved;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(instructorAssignment);
                            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(instructorAssignment.SubjectId);
                            // Send notification to the requester
                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Instructor Assignment Approved",
                                $"Your instructor assignment request has been approved. Assignment ID: {instructorAssignment.AssignmentId}",
                                "InstructorAssignment"
                            ));

                            _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                                instructorAssignment.InstructorId,
                                "New Instructor Assignment",
                                $"You has been assign to teach subject {subject.SubjectName}",
                                "InstructorAssignment"
                            ));
                        }
                        actionSuccessful = true;
                        break;

                    default:
                        // Handle default case
                        actionSuccessful = true;
                        break;
                }

                // Update status only after all actions completed successfully
                if (actionSuccessful)
                {
                    request.Status = RequestStatus.Approved;

                    // Notify the requester about the approval
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        request.RequestUserId,
                        "Request Approved",
                        $"Your request ({request.RequestType}) has been approved.",
                        "Request"
                    ));

                    await _unitOfWork.RequestRepository.UpdateAsync(request);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error when approving request {requestId}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                // Re-throw the exception to be handled by the API controller
                throw;
            }
        }
        #endregion

        #region Reject Request
        public async Task<bool> RejectRequestAsync(string requestId, string rejectionReason, string rejectByUserId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return false;

            request.UpdatedAt = DateTime.Now;
            request.ApproveByUserId = rejectByUserId;
            request.ApprovedDate = DateTime.Now;

            if (request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Rejected;
            }
            else if (request.Status == RequestStatus.Approved)
            {
                throw new InvalidOperationException("The request has already been approved and cannot be rejected.");
            }
            else
            {
                throw new InvalidOperationException("The request cannot be rejected in its current status.");
            }

            // Tailor notification message based on RequestType
            string notificationTitle = "Request Rejected";
            string notificationMessage;
            var rejecter = await _userRepository.GetByIdAsync(rejectByUserId);

            switch (request.RequestType)
            {
                case RequestType.NewPlan:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }

                    // Simplified rejection logic - no direct TrainingPlanRepository usage
                    notificationMessage = $"Your training plan request (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.Update:
                //case RequestType.PlanChange:
                //    if (rejecter == null || rejecter.RoleId != 2)
                //    {
                //        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                //    }

                //    notificationMessage = $"Your request to update training plan (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                //    break;

                case RequestType.Delete:
                //case RequestType.PlanDelete:
                //    if (rejecter == null || rejecter.RoleId != 2)
                //    {
                //        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                //    }

                //    notificationMessage = $"Your request to delete training plan (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                //    break;

                case RequestType.CandidateImport:
                    if (rejecter == null || rejecter.RoleId != 3)
                    {
                        throw new UnauthorizedAccessException("Only TrainingStaff can reject this request.");
                    }

                    notificationMessage = $"Your candidate import request has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.AssignTrainee:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }

                    var traineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync(
                        t => t.RequestId == request.RequestId);

                    foreach (var assign in traineeAssigns)
                    {
                        assign.RequestStatus = RequestStatus.Rejected;
                        assign.ApprovalDate = null;
                        assign.ApproveByUserId = null;
                        await _unitOfWork.TraineeAssignRepository.UpdateAsync(assign);
                    }

                    notificationMessage = $"Your request to assign trainees has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.AddTraineeAssign:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }

                    var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(
                        t => t.RequestId == request.RequestId);

                    if (traineeAssign != null)
                    {
                        traineeAssign.RequestStatus = RequestStatus.Rejected;
                        traineeAssign.ApprovalDate = null;
                        traineeAssign.ApproveByUserId = null;
                        await _unitOfWork.TraineeAssignRepository.UpdateAsync(traineeAssign);
                    }

                    notificationMessage = $"Your request to assign a trainee has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.DecisionTemplate:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var template = await _unitOfWork.DecisionTemplateRepository.GetByIdAsync(request.RequestEntityId);
                    if (template != null)
                    {
                        template.TemplateStatus = (int)TemplateStatus.Inactive;
                        await _unitOfWork.DecisionTemplateRepository.UpdateAsync(template);
                    }

                    notificationMessage = $"Your request to approve the template has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.CertificateTemplate:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var certiTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(request.RequestEntityId);
                    if (certiTemplate != null)
                    {
                        certiTemplate.templateStatus = (int)TemplateStatus.Inactive;
                        await _unitOfWork.CertificateTemplateRepository.UpdateAsync(certiTemplate);
                    }

                    notificationMessage = $"Your request to approve the template has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.Revoke:
                    if (rejecter == null || rejecter.RoleId != 3)
                    {
                        throw new UnauthorizedAccessException("Only TrainingStaff can reject this request.");
                    }
                    var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(request.RequestEntityId);
                    if (certificate != null)
                    {
                        certificate.Status = CertificateStatus.Pending;
                        await _unitOfWork.CertificateRepository.UpdateAsync(certificate);
                    }

                    notificationMessage = $"Your request to approve the revoke certificate {certificate.CertificateCode} has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.NewCourse:
                case RequestType.UpdateCourse:
                case RequestType.DeleteCourse:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }

                    var course = await _unitOfWork.CourseRepository.GetByIdAsync(request.RequestEntityId);
                    if (course != null)
                    {
                        if (request.RequestType == RequestType.NewCourse)
                        {
                            course.Status = CourseStatus.Rejected;
                        }
                        else
                        {
                            // For update/delete, keep the status as approved
                            course.Status = CourseStatus.Approved;
                        }

                        await _unitOfWork.CourseRepository.UpdateAsync(course);
                    }

                    notificationMessage = $"Your request for course {course?.CourseName ?? request.RequestEntityId} ({request.RequestType}) has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.ClassSchedule:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }

                    var rejectedSchedule = await _unitOfWork.TrainingScheduleRepository.GetByIdAsync(request.RequestEntityId);
                    if (rejectedSchedule != null)
                    {
                        rejectedSchedule.Status = ScheduleStatus.Canceled;
                        rejectedSchedule.Notes = rejectionReason;
                        await _unitOfWork.TrainingScheduleRepository.UpdateAsync(rejectedSchedule);
                    }

                    notificationMessage = $"Your schedule request has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.AssignInstructor:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }

                    var rejectedAssignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(request.RequestEntityId);
                    if (rejectedAssignment != null)
                    {
                        rejectedAssignment.RequestStatus = RequestStatus.Rejected;
                        await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(rejectedAssignment);
                    }

                    notificationMessage = $"Your instructor assignment request has been rejected. Reason: {rejectionReason}";
                    break;

                default:
                    notificationMessage = $"Your request ({request.RequestType}) has been rejected. Reason: {rejectionReason}";
                    break;
            }

            // Send notification to the request creator
            await _notificationService.SendNotificationAsync(
                request.RequestUserId,
                notificationTitle,
                notificationMessage,
                "Request"
            );

            // Additional logic for CandidateImport
            if (request.RequestType == RequestType.CandidateImport)
            {
                var candidates = await _candidateRepository.GetCandidatesByImportRequestIdAsync(requestId);
                if (candidates != null && candidates.Any())
                {
                    foreach (var candidate in candidates)
                    {
                        candidate.CandidateStatus = CandidateStatus.Rejected;
                        await _unitOfWork.CandidateRepository.UpdateAsync(candidate);
                    }
                }

                var hrs = await _userRepository.GetUsersByRoleAsync("HR");
                foreach (var hr in hrs)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.SendNotificationAsync(
                        hr.UserId,
                        "Candidate Import Rejected",
                        $"The candidate import request has been rejected. Reason: {rejectionReason}",
                        "CandidateImport"
                    ));
                }
            }

            await _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
