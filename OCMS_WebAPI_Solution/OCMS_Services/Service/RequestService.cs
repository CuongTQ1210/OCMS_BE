using AutoMapper;
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
        private readonly Lazy<ITrainingPlanService> _trainingPlanService;
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorAssignmentRepository _instructorAssignmentRepository;
        private readonly ITraineeAssignRepository _traineeAssignRepository;
        public RequestService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            Lazy<ITrainingScheduleService> trainingScheduleService,
            Lazy<ITrainingPlanService> trainingPlanService, ITrainingScheduleRepository trainingScheduleRepository, ICourseRepository courseRepository, IInstructorAssignmentRepository instructorAssignmentRepository, ITraineeAssignRepository traineeAssignRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
            _trainingScheduleRepository = trainingScheduleRepository;
            _courseRepository = courseRepository;
            _instructorAssignmentRepository = instructorAssignmentRepository;
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
            _trainingPlanService = trainingPlanService ?? throw new ArgumentNullException(nameof(trainingPlanService));
            _traineeAssignRepository = traineeAssignRepository;
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
                RequestUser= user,
                RequestEntityId = requestDto.RequestEntityId,
                Status=RequestStatus.Pending,
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
                newRequest.RequestType == RequestType.Update||
                newRequest.RequestType == RequestType.Delete||
                newRequest.RequestType== RequestType.AssignTrainee||
                newRequest.RequestType == RequestType.AddTraineeAssign||
                newRequest.RequestType== RequestType.SignRequest
                )
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "Request"
                    );
                }
            }
            if ( newRequest.RequestType == RequestType.SignRequest)
                
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} for certificateId {requestDto.RequestEntityId} need to be signed.",
                        "Request"
                    );
                }
            }
            else if(newRequest.RequestType == RequestType.CreateNew ||
                newRequest.RequestType == RequestType.CreateRecurrent ||
                newRequest.RequestType == RequestType.CreateRelearn)
            {
                
                var eduofficers = await _userRepository.GetUsersByRoleAsync("Training staff");
                
                    foreach (var edu in eduofficers)
                {
                    await _notificationService.SendNotificationAsync(
                        edu.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "Request"
                    );
                }
            }

            if (newRequest.RequestType == RequestType.CandidateImport)
            {
                var staffs = await _userRepository.GetUsersByRoleAsync("Training staff");
                foreach (var staff in staffs)
                {
                    await _notificationService.SendNotificationAsync(
                        staff.UserId,
                        "New Candidate Import Request",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "CandidateImport"
                    );
                }
            }

            if (newRequest.RequestType == RequestType.DecisionTemplate)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Template Approval Request",
                        "A new template approval request has been submitted for review.",
                        "TemplateApprove"
                    );
                }
            }
            if (newRequest.RequestType == RequestType.CertificateTemplate)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Template Approval Request",
                        "A new template approval request has been submitted for review.",
                        "TemplateApprove"
                    );
                }
            }
            if (newRequest.RequestType == RequestType.Revoke)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "Revoke Certificate Approval Request",
                        "A new Revoke approval request has been submitted for review.",
                        "RevokeCertificate"
                    );
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
        RequestType.AddTraineeAssign,
        RequestType.AssignTrainee,
        RequestType.DecisionTemplate,
        RequestType.CertificateTemplate,
        RequestType.PlanChange,
        RequestType.PlanDelete,
        RequestType.Update,
        RequestType.Delete,
        RequestType.SignRequest
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

                    var plan = await _unitOfWork.TrainingPlanRepository
                        .GetQueryable()
                        .Where(p => p.PlanId == entityId)
                        .Include(p => p.Course)
                            .ThenInclude(c => c.CourseSubjectSpecialties)
                                .ThenInclude(css => css.Subject)
                        .Include(p => p.Course)
                            .ThenInclude(c => c.CourseSubjectSpecialties)
                                .ThenInclude(css => css.Schedules)
                        .Include(p => p.Course)
                            .ThenInclude(c => c.CourseSubjectSpecialties)
                                .ThenInclude(css => css.Trainees)
                        .FirstOrDefaultAsync();

                    if (plan == null)
                    {
                        throw new KeyNotFoundException("Training plan not found.");
                    }

                    // Step 1: Has at least one course
                    if (plan.Course == null)
                    {
                        throw new InvalidOperationException("Training plan must have at least one course.");
                    }

                        // Step 2: Each course has at least one CourseSubjectSpecialty
                        if (plan.Course.CourseSubjectSpecialties == null || !plan.Course.CourseSubjectSpecialties.Any())
                        {
                            throw new InvalidOperationException($"Course '{plan.Course.CourseName}' must have at least one CourseSubjectSpecialty.");
                        }

                        foreach (var css in plan.Course.CourseSubjectSpecialties)
                        {
                            // Step 2a: Ensure CourseSubjectSpecialty has a Subject
                            if (css.Subject == null)
                            {
                                throw new InvalidOperationException($"CourseSubjectSpecialty with ID '{css.Id}' in course '{plan.Course.CourseName}' must be associated with a subject.");
                            }

                            // Step 3: Each CourseSubjectSpecialty has at least one schedule
                            if (css.Schedules == null || !css.Schedules.Any())
                            {
                                throw new InvalidOperationException($"Subject '{css.Subject.SubjectName}' in course '{plan.Course.CourseName}' must have at least one schedule.");
                            }
                        }
                    

                    return true;
                case RequestType.Update:
                case RequestType.Delete:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                        
                    // Kiểm tra format "CourseId:...specialtyId:..." cho việc xóa tất cả subject trong course và specialty
                    if (entityId.StartsWith("CourseId:") && entityId.Contains("specialtyId:"))
                    {
                        // Phân tích RequestEntityId để lấy CourseId và SpecialtyId
                        string fullId = entityId;
                        int specialtyIdIndex = fullId.IndexOf("specialtyId:");
                        
                        if (specialtyIdIndex > 0)
                        {
                            string courseIdPart = fullId.Substring(9, specialtyIdIndex - 9);
                            string specialtyIdPart = fullId.Substring(specialtyIdIndex + 12);
                            
                            // Kiểm tra tồn tại của Course và Specialty
                            bool courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == courseIdPart);
                            bool specialtyExists = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyIdPart);
                            
                            return courseExists && specialtyExists;
                        }
                        
                        return false;
                    }
                    
                    // Kiểm tra format "{CourseId}:{SpecialtyId}" cho việc xóa tất cả subject trong course và specialty (format mới)
                    if (entityId.Contains(":") && entityId.Split(':').Length == 2)
                    {
                        string[] parts = entityId.Split(':');
                        string courseId = parts[0];
                        string specialtyId = parts[1];
                        
                        // Kiểm tra tồn tại của Course và Specialty
                        bool courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == courseId);
                        bool specialtyExists = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyId);
                        
                        // Kiểm tra xem có CourseSubjectSpecialty nào tồn tại cho cặp này không
                        bool hasSubjects = await _unitOfWork.CourseSubjectSpecialtyRepository.ExistsAsync(
                            css => css.CourseId == courseId && css.SpecialtyId == specialtyId);
                            
                        if (!hasSubjects)
                            throw new InvalidOperationException($"No subjects found for Course: {courseId} - Specialty: {specialtyId}");
                            
                        return courseExists && specialtyExists;
                    }
                    
                    // Kiểm tra format "{CourseId}:{SubjectId}:{SpecialtyId}" cho việc thêm subject vào course đã approved
                    if (entityId.Contains(":") && entityId.Split(':').Length == 3)
                    {
                        string[] parts = entityId.Split(':');
                        string courseId = parts[0];
                        string subjectId = parts[1];
                        string specialtyId = parts[2];
                        
                        bool courseExists = await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == courseId);
                        bool subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == subjectId);
                        bool specialtyExists = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyId);
                        
                        // Kiểm tra xem combination này đã tồn tại chưa
                        bool combinationExists = await _unitOfWork.CourseSubjectSpecialtyRepository.ExistsAsync(
                            css => css.CourseId == courseId && css.SubjectId == subjectId && css.SpecialtyId == specialtyId);
                            
                        // Nếu đã tồn tại, không cho phép thêm request
                        if (combinationExists)
                            throw new InvalidOperationException($"Course-Subject-Specialty combination already exists.");
                            
                        return courseExists && subjectExists && specialtyExists;
                    }
                    
                    var validplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(entityId);
                    if (validplan != null && validplan.TrainingPlanStatus != TrainingPlanStatus.Approved)
                    {
                        throw new InvalidOperationException("The plan has not been approved yet you can update or delete it if needed.");
                    }

                    return await _unitOfWork.TrainingPlanRepository.ExistsAsync(tp => tp.PlanId == entityId);
                case RequestType.PlanChange:
                case RequestType.PlanDelete:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    var trainingplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(entityId);
                    if (trainingplan != null && trainingplan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        throw new InvalidOperationException("The request has already been approved and cannot send request.");
                    }

                    return await _unitOfWork.TrainingPlanRepository.ExistsAsync(tp => tp.PlanId == entityId);

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
                    var existedCertificate = await _unitOfWork.CertificateRepository.ExistsAsync(dt => dt.CertificateId == entityId); 
                    if( existedCertificate)
                    {
                        return true;
                    }
                    return false;
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
                
                // Lưu các thông tin cơ bản nhưng chưa đổi status
                request.ApproveByUserId = approvedByUserId;
                request.ApprovedDate = DateTime.Now;
                request.UpdatedAt = DateTime.Now;
                
                // Biến để kiểm tra xem tất cả các hoạt động đã hoàn thành thành công chưa
                bool actionSuccessful = false;

                // Handle request type-specific actions
                switch (request.RequestType)
            {
                case RequestType.NewPlan:
                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }

                    var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (plan != null)
                    {
                        plan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                        plan.ApproveByUserId = approvedByUserId;
                        plan.ApproveDate = DateTime.Now;
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);

                        var course = await _courseRepository.GetCourseByTrainingPlanIdAsync(plan.PlanId);
                        
                            course.Status = CourseStatus.Approved;
                            course.Progress = Progress.Ongoing;
                            course.ApproveByUserId = approvedByUserId;
                            course.ApprovalDate = DateTime.Now;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);

                            var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository
                                .GetAllAsync(css => css.CourseId == course.CourseId,
                                    css => css.Schedules,
                                    css => css.Trainees);
                            foreach (var css in courseSubjectSpecialties)
                            {
                                foreach (var schedule in css.Schedules)
                                {
                                    schedule.Status = ScheduleStatus.Approved;
                                    schedule.ModifiedDate = DateTime.Now;
                                    await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                                }

                                foreach (var trainee in css.Trainees)
                                {
                                    trainee.RequestStatus = RequestStatus.Approved;
                                    await _unitOfWork.TraineeAssignRepository.UpdateAsync(trainee);
                                    await _notificationService.SendNotificationAsync(
                                        trainee.TraineeId,
                                        "Course Assignment Approved",
                                        $"You have been assigned to CourseSubjectSpecialty {css.Id}.",
                                        "Training assignment notification."
                                    );
                                }
                            }
                        

                        var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var assignment in instructorAssignments)
                        {
                            assignment.RequestStatus = RequestStatus.Approved;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                            await _notificationService.SendNotificationAsync(
                                assignment.InstructorId,
                                "Subject schedule assignment",
                                $"You have been assigned to this subject {assignment.CourseSubjectSpecialty.Subject.SubjectName}.",
                                "Schedule notification."
                            );
                        }

                        if (!string.IsNullOrEmpty(request.RequestUserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Trainee Plan Approved",
                                $"Your request for {request.RequestEntityId} has been approved.",
                                $"{request.RequestType.ToString()}"
                            );
                        }
                    }
                        
                        actionSuccessful = true;
                    break;
                        
                case RequestType.Update:
                case RequestType.Delete:
                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }

                        // check if the request is to delete all subjects for a course and specialty
                        if (request.RequestEntityId.StartsWith("CourseId:") && request.RequestEntityId.Contains("specialtyId:"))
                        {
                            string fullId = request.RequestEntityId;
                            int specialtyIdIndex = fullId.IndexOf("specialtyId:");
                            
                            string courseIdPart = fullId.Substring(9, specialtyIdIndex - 9);
                            string specialtyIdPart = fullId.Substring(specialtyIdIndex + 12);
                            
                            // get the list of CourseSubjectSpecialty to delete
                            var cssItems = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                                css => css.CourseId == courseIdPart && css.SpecialtyId == specialtyIdPart,
                                css => css.Schedules
                            );
                            
                            if (cssItems == null || !cssItems.Any())
                            {
                                throw new KeyNotFoundException($"No subjects found for Course: {courseIdPart} - Specialty: {specialtyIdPart}");
                            }
                            
                            foreach (var css in cssItems)
                            {
                                foreach (var schedule in css.Schedules)
                                {
                                    await _trainingScheduleService.Value.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                                }
                                await _unitOfWork.CourseSubjectSpecialtyRepository.DeleteAsync(css.Id);
                            }
                            
                            await _unitOfWork.SaveChangesAsync();
                            await _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Delete Request Approved",
                                $"Your request to delete all subjects for Course: {courseIdPart} - Specialty: {specialtyIdPart} has been approved.",
                                "Delete Subjects"
                            );
                            
                            actionSuccessful = true;
                            break;
                        }
                        
                        // Check if the request is to delete all subjects for a course and specialty with the format "{CourseId}:{SpecialtyId}"
                        if (request.RequestEntityId.Contains(":") && request.RequestEntityId.Split(':').Length == 2)
                        {
                            string[] parts = request.RequestEntityId.Split(':');
                            string courseId = parts[0];
                            string specialtyId = parts[1];
                            
                            // get the list of CourseSubjectSpecialty to delete
                            var cssItems = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                                css => css.CourseId == courseId && css.SpecialtyId == specialtyId,
                                css => css.Schedules
                            );
                            
                            if (cssItems == null || !cssItems.Any())
                            {
                                throw new KeyNotFoundException($"No subjects found for Course: {courseId} - Specialty: {specialtyId}");
                            }
                            
                            foreach (var css in cssItems)
                            {
                                foreach (var schedule in css.Schedules)
                                {
                                    await _trainingScheduleService.Value.DeleteTrainingScheduleAsync(schedule.ScheduleID);
                                }
                                await _unitOfWork.CourseSubjectSpecialtyRepository.DeleteAsync(css.Id);
                            }
                            
                            await _unitOfWork.SaveChangesAsync();
                            await _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Delete Request Approved",
                                $"Your request to delete all subjects for Course: {courseId} - Specialty: {specialtyId} has been approved.",
                                "Delete Subjects"
                            );
                            
                            actionSuccessful = true;
                            break;
                        }
                        
                        // Check if the request is to add subject to approved course with the format "{CourseId}:{SubjectId}:{SpecialtyId}"
                        if (request.RequestType == RequestType.Update && request.RequestEntityId.Contains(":") && request.RequestEntityId.Split(':').Length == 3)
                        {
                            string[] parts = request.RequestEntityId.Split(':');
                            string courseId = parts[0];
                            string subjectId = parts[1];
                            string specialtyId = parts[2];
                            
                            // check if the course, subject, specialty exists
                            var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
                            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(subjectId);
                            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(specialtyId);
                            
                            if (course == null)
                            {
                                throw new KeyNotFoundException($"Course with ID '{courseId}' not found.");
                            }
                            
                            if (subject == null)
                            {
                                throw new KeyNotFoundException($"Subject with ID '{subjectId}' not found.");
                            }
                            
                            if (specialty == null)
                            {
                                throw new KeyNotFoundException($"Specialty with ID '{specialtyId}' not found.");
                            }
                            
                            // Verify if the subject is suitable for this course (add your business logic here)
                            // For example, check if the subject fits into the course curriculum
                            
                            // check if the combination already exists
                            bool exists = await _unitOfWork.CourseSubjectSpecialtyRepository.ExistsAsync(
                                css => css.CourseId == courseId && css.SubjectId == subjectId && css.SpecialtyId == specialtyId);
                                
                            if (exists)
                            {
                                throw new InvalidOperationException($"Course-Subject-Specialty combination already exists for Course: {courseId}, Subject: {subjectId}, Specialty: {specialtyId}");
                            }
                            
                            // create new CourseSubjectSpecialty
                            var css = new CourseSubjectSpecialty
                            {
                                Id = Guid.NewGuid().ToString(),
                                CourseId = courseId,
                                SubjectId = subjectId,
                                SpecialtyId = specialtyId,
                                CreatedByUserId = request.RequestUserId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                Notes = "HeadMasterApprove"
                            };
                            
                            await _unitOfWork.CourseSubjectSpecialtyRepository.AddAsync(css);
                            await _unitOfWork.SaveChangesAsync();
                            
                            await _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Add Subject Request Approved",
                                $"Your request to add subject {subjectId} to course {courseId} with specialty {specialtyId} has been approved.",
                                "Add Subject"
                            );
                            
                            actionSuccessful = true;
                            break;
                        }

                    var trainingplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingplan != null)
                    {
                        trainingplan.TrainingPlanStatus = TrainingPlanStatus.Pending;
                        trainingplan.ApproveByUserId = approvedByUserId;
                        trainingplan.ApproveDate = DateTime.UtcNow;
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(trainingplan);

                        var course = await _courseRepository.GetCourseByTrainingPlanIdAsync(trainingplan.PlanId);
                        
                            course.Status = CourseStatus.Pending;
                            course.Progress = Progress.NotYet;
                            course.ApproveByUserId = approvedByUserId;
                            course.ApprovalDate = DateTime.Now;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);

                            var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository
                                .GetAllAsync(css => css.CourseId == course.CourseId,
                                    css => css.Schedules,
                                    css => css.Trainees);
                            foreach (var css in courseSubjectSpecialties)
                            {
                                foreach (var schedule in css.Schedules)
                                {
                                    schedule.Status = ScheduleStatus.Pending;
                                    schedule.ModifiedDate = DateTime.Now;
                                    await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                                }

                                foreach (var trainee in css.Trainees)
                                {
                                    trainee.RequestStatus = RequestStatus.Pending;
                                    trainee.ApprovalDate = DateTime.Now;
                                    await _unitOfWork.TraineeAssignRepository.UpdateAsync(trainee);
                                }
                            }
                        

                        var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(trainingplan.PlanId);
                        foreach (var assignment in instructorAssignments)
                        {
                            assignment.RequestStatus = RequestStatus.Pending;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                        }

                        if (!string.IsNullOrEmpty(request.RequestUserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Training Plan Status Updated",
                                $"Your request for {request.RequestEntityId} has been set to pending for {request.RequestType.ToString().ToLower()}.",
                                $"{request.RequestType.ToString()}"
                            );
                        }
                            
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
                            }

                            request.Status = RequestStatus.Approved;
                            await _unitOfWork.RequestRepository.UpdateAsync(request);

                            await _unitOfWork.SaveChangesAsync();

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

                            request.Status = RequestStatus.Approved;

                            // Save changes
                            await _unitOfWork.TraineeAssignRepository.UpdateAsync(traineeAssign);
                            await _unitOfWork.RequestRepository.UpdateAsync(request);
                            await _unitOfWork.SaveChangesAsync();

                            break;
                        }
                    default:
                        // Handle default case
                        actionSuccessful = true;
                    break;
                }
                
                // Chỉ cập nhật status sau khi tất cả các hoạt động đã hoàn thành thành công
                if (actionSuccessful)
                {
                    request.Status = RequestStatus.Approved;
                    
                    // Notify the requester about the approval
                        await _notificationService.SendNotificationAsync(
                        request.RequestUserId,
                        "Request Approved",
                        $"Your request ({request.RequestType}) has been approved.",
                        "Request"
                    );
                    
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
            
            request.UpdatedAt = DateTime.Now;
            request.ApproveByUserId = rejectByUserId;
            request.ApprovedDate = DateTime.Now;
            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Rejected;
            }
            else if (request != null && request.Status == RequestStatus.Approved)
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
                    var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (plan != null)
                    {
                        plan.TrainingPlanStatus = TrainingPlanStatus.Rejected;
                        plan.ApproveByUserId = null;
                        plan.ApproveDate = null;
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);

                        // ❌ Reject all associated courses
                        var course = await _courseRepository.GetCourseByTrainingPlanIdAsync(plan.PlanId);
                        
                            course.Status = CourseStatus.Rejected;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);

                            // Load CourseSubjectSpecialties for the course
                            var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository
                                .GetAllAsync(css => css.CourseId == course.CourseId,
                                    css => css.Schedules,
                                    css => css.Trainees);
                            foreach (var css in courseSubjectSpecialties)
                            {
                                // Reject all schedules
                                foreach (var schedule in css.Schedules)
                                {
                                    schedule.Status = ScheduleStatus.Canceled;
                                    schedule.ModifiedDate = DateTime.UtcNow;
                                    await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                                }

                                // Reject all trainee assignments
                                foreach (var trainee in css.Trainees)
                                {
                                    trainee.RequestStatus = RequestStatus.Rejected;
                                    trainee.ApprovalDate = null;
                                    trainee.ApproveByUserId = null;
                                    await _unitOfWork.TraineeAssignRepository.UpdateAsync(trainee);
                                }
                            }
                        

                        // ❌ Reject all instructor assignments
                        var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var assignment in instructorAssignments)
                        {
                            assignment.RequestStatus = RequestStatus.Rejected;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                        }
                    }

                    notificationMessage = $"Your training plan request (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.Update:
                case RequestType.PlanChange:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingPlan != null)
                    {
                        // If the plan is Approved, keep it Approved but reject associated requests
                        if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                        {
                            trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Approved;

                            // Reject associated entities
                            var course = await _courseRepository.GetCourseByTrainingPlanIdAsync(trainingPlan.PlanId);
                            
                                // If course was pending update, revert to previous state
                                course.Status = CourseStatus.Approved;
                                await _unitOfWork.CourseRepository.UpdateAsync(course);

                                var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository
                                    .GetAllAsync(css => css.CourseId == course.CourseId,
                                        css => css.Schedules,
                                        css => css.Trainees);
                                foreach (var css in courseSubjectSpecialties)
                                {
                                    foreach (var schedule in css.Schedules)
                                    {
                                        schedule.Status = ScheduleStatus.Approved;
                                        schedule.ModifiedDate = DateTime.UtcNow;
                                        await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                                    }

                                    foreach (var trainee in css.Trainees)
                                    {
                                        trainee.RequestStatus = RequestStatus.Approved;
                                        await _unitOfWork.TraineeAssignRepository.UpdateAsync(trainee);
                                    }
                                }
                            

                            var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(trainingPlan.PlanId);
                            foreach (var assignment in instructorAssignments)
                            {
                                assignment.RequestStatus = RequestStatus.Approved;
                                await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                            }
                        }
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(trainingPlan);
                    }

                    notificationMessage = $"Your request to update training plan (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                    break;

                case RequestType.Delete:
                case RequestType.PlanDelete:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var _trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (_trainingPlan != null)
                    {
                        // If the plan is Approved, keep it Approved but reject associated requests
                        if (_trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                        {
                            _trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Approved;

                            var course = await _courseRepository.GetCourseByTrainingPlanIdAsync(_trainingPlan.PlanId);
                            
                                course.Status = CourseStatus.Approved;
                                await _unitOfWork.CourseRepository.UpdateAsync(course);

                                var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository
                                    .GetAllAsync(css => css.CourseId == course.CourseId,
                                        css => css.Schedules,
                                        css => css.Trainees);
                                foreach (var css in courseSubjectSpecialties)
                                {
                                    foreach (var schedule in css.Schedules)
                                    {
                                        schedule.Status = ScheduleStatus.Approved;
                                        schedule.ModifiedDate = DateTime.UtcNow;
                                        await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                                    }

                                    foreach (var trainee in css.Trainees)
                                    {
                                        trainee.RequestStatus = RequestStatus.Approved;
                                        await _unitOfWork.TraineeAssignRepository.UpdateAsync(trainee);
                                    }
                                }
                            

                            var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(_trainingPlan.PlanId);
                            foreach (var assignment in instructorAssignments)
                            {
                                assignment.RequestStatus = RequestStatus.Approved;
                                await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                            }
                        }
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(_trainingPlan);
                    }

                    notificationMessage = $"Your request to delete training plan (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                    break;

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
                        t => t.RequestId == request.RequestId,
                        t => t.CourseSubjectSpecialty,
                        t => t.CourseSubjectSpecialty.Course);
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
                        t => t.RequestId == request.RequestId,
                        t => t.CourseSubjectSpecialty,
                        t => t.CourseSubjectSpecialty.Course);
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
                    await _notificationService.SendNotificationAsync(
                        hr.UserId,
                        "Candidate Import Rejected",
                        $"The candidate import request has been rejected. Reason: {rejectionReason}",
                        "CandidateImport"
                    );
                }
            }
            await _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
