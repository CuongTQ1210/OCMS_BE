﻿using AutoMapper;
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
        
        public RequestService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            Lazy<ITrainingScheduleService> trainingScheduleService,
            Lazy<ITrainingPlanService> trainingPlanService, ITrainingScheduleRepository trainingScheduleRepository, ICourseRepository courseRepository, IInstructorAssignmentRepository instructorAssignmentRepository)
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
            if (!string.IsNullOrEmpty(requestDto.RequestEntityId))
            {
                bool isValidEntity = await ValidateRequestEntityIdAsync(requestDto.RequestType, requestDto.RequestEntityId);
                if (!isValidEntity)
                    throw new ArgumentException("Invalid RequestEntityId for the given RequestType.");
            }

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
                RequestDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ApprovedBy = null,
                ApprovedDate = null
            };

            await _unitOfWork.RequestRepository.AddAsync(newRequest);
            await _unitOfWork.SaveChangesAsync();

            // 🚀 Send notification to the director if NewPlan, RecurrentPlan, RelearnPlan
            if (newRequest.RequestType == RequestType.NewPlan ||
                newRequest.RequestType == RequestType.RecurrentPlan ||
                newRequest.RequestType == RequestType.RelearnPlan||
                newRequest.RequestType == RequestType.Update||
                newRequest.RequestType == RequestType.Delete||
                newRequest.RequestType== RequestType.AssignTrainee||
                newRequest.RequestType == RequestType.AddTraineeAssign

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
            else if(newRequest.RequestType == RequestType.CreateNew ||
                newRequest.RequestType == RequestType.CreateRecurrent ||
                newRequest.RequestType == RequestType.CreateRelearn)
            {
                var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(requestDto.RequestEntityId)
    ?? throw new KeyNotFoundException("Plan not found!");

                plan.TrainingPlanStatus = TrainingPlanStatus.Pending;
                await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);
                await _unitOfWork.SaveChangesAsync(); 
                var directors = await _userRepository.GetUsersByRoleAsync("Training staff");
                
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

            if (newRequest.RequestType == RequestType.CandidateImport)
            {
                var admins = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var admin in admins)
                {
                    await _notificationService.SendNotificationAsync(
                        admin.UserId,
                        "New Candidate Import Request",
                        "A new candidate import request has been submitted for review.",
                        "CandidateImport"
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
        private string GenerateRequestId()
        {
            return $"REQ-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }
        private async Task<bool> ValidateRequestEntityIdAsync(RequestType requestType, string entityId)
        {
            switch (requestType)
            {
                case RequestType.NewPlan:
                case RequestType.RecurrentPlan:
                case RequestType.RelearnPlan:
                case RequestType.PlanChange:
                case RequestType.PlanDelete:
                case RequestType.CreateNew:
                case RequestType.CreateRecurrent:
                case RequestType.CreateRelearn:
                    return await _unitOfWork.TrainingPlanRepository.ExistsAsync(tp => tp.PlanId == entityId);

                case RequestType.Complaint:
                    return await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == entityId);

                default:
                    return false; // Invalid type
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
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Approved;
            request.ApprovedBy = approvedByUserId;
            request.ApprovedDate = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // Notify the requester
            await _notificationService.SendNotificationAsync(
                request.RequestUserId,
                "Request Approved",
                $"Your request ({request.RequestType}) has been approved.",
                "Request"
            );

            // Handle request type-specific actions
            switch (request.RequestType)
            {
                case RequestType.NewPlan:
                case RequestType.RecurrentPlan:
                case RequestType.RelearnPlan:
                    var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (plan != null)
                    {
                        plan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                        plan.ApproveByUserId = approvedByUserId;
                        plan.ApproveDate = DateTime.UtcNow;
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);
                        // ✅ Approve all courses in the plan
                        var courses = await _courseRepository.GetCoursesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var course in courses)
                        {
                            course.Status = CourseStatus.Approved;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);
                        }

                        // ✅ Approve all schedules
                        var schedules = await _trainingScheduleRepository.GetSchedulesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var schedule in schedules)
                        {
                            schedule.Status = ScheduleStatus.Incoming;
                            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                        }

                        // ✅ Approve all instructor assignments
                        var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var assignment in instructorAssignments)
                        {
                            assignment.RequestStatus = RequestStatus.Approved;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
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
                    break;
                case RequestType.CandidateImport:
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
                        await _notificationService.SendNotificationAsync(
                            admin.UserId,
                            "Candidate Import Approved",
                            "The candidate import request has been approved. Please create user accounts for the new candidates.",
                            "CandidateImport"
                        );
                    }
                    break;
                case RequestType.AssignTrainee:
                    var traineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync(t => t.RequestId == requestId);
                    if (traineeAssigns == null || !traineeAssigns.Any())
                        return false;

                    foreach (var assign in traineeAssigns)
                    {
                        assign.RequestStatus = RequestStatus.Approved;
                        assign.ApprovalDate = DateTime.UtcNow;
                        assign.ApproveByUserId = approvedByUserId;
                        await _unitOfWork.TraineeAssignRepository.UpdateAsync(assign);

                        // ✅ Notify the trainee
                        await _notificationService.SendNotificationAsync(
                            assign.TraineeId,
                            "Trainee Assignment Approved",
                            $"You have been assigned to Course {assign.CourseId}.",
                            "TraineeAssign"
                        );

                        // ✅ Notify the assigner (AssignByUserId)
                        if (!string.IsNullOrEmpty(assign.AssignByUserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                assign.AssignByUserId,
                                "Trainee Assignment Approved",
                                $"Your request to assign {assign.TraineeId} to Course {assign.CourseId} has been approved.",
                                "TraineeAssign"
                            );
                        }
                    }
                    break;

                case RequestType.AddTraineeAssign:
                    var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(t => t.RequestId == requestId);
                    if (traineeAssign == null)
                        return false;

                    traineeAssign.RequestStatus = RequestStatus.Approved;
                    traineeAssign.ApprovalDate = DateTime.UtcNow;
                    traineeAssign.ApproveByUserId = approvedByUserId;
                    await _unitOfWork.TraineeAssignRepository.UpdateAsync(traineeAssign);

                    // ✅ Notify the trainee
                    await _notificationService.SendNotificationAsync(
                        traineeAssign.TraineeId,
                        "Trainee Assignment Approved",
                        $"You have been assigned to Course {traineeAssign.CourseId}.",
                        "TraineeAssign"
                    );

                    // ✅ Notify the assigner (AssignByUserId)
                    if (!string.IsNullOrEmpty(traineeAssign.AssignByUserId))
                    {
                        await _notificationService.SendNotificationAsync(
                            traineeAssign.AssignByUserId,
                            "Trainee Assignment Approved",
                            $"Your request to assign {traineeAssign.TraineeId} to Course {traineeAssign.CourseId} has been approved.",
                            "TraineeAssign"
                        );
                    }
                    break;
                case RequestType.Update:
                    var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingPlan != null && trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        // Extract proposed changes from Notes
                        if (request.Notes != null && request.Notes.Contains("Proposed changes:"))
                        {
                            var jsonStart = request.Notes.IndexOf("{");
                            var jsonEnd = request.Notes.LastIndexOf("}") + 1;
                            if (jsonStart >= 0 && jsonEnd > jsonStart)
                            {
                                var json = request.Notes.Substring(jsonStart, jsonEnd - jsonStart);
                                var dto = JsonSerializer.Deserialize<TrainingPlanDTO>(json);

                                // Apply the changes
                                trainingPlan.PlanName = dto.PlanName;
                                trainingPlan.Desciption = dto.Desciption;
                                trainingPlan.ModifyDate = DateTime.UtcNow;
                                trainingPlan.CreateByUserId = request.RequestUserId; // Or approvedByUserId
                                trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                                await _unitOfWork.TrainingPlanRepository.UpdateAsync(trainingPlan);
                            }
                        }
                    }
                    
                    break;

                case RequestType.Delete:
                    
                    var trainingPlanToDelete = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingPlanToDelete != null)
                    {
                        await _trainingPlanService.Value.DeleteTrainingPlanAsync(request.RequestEntityId);
                    }
                    break;

                    
            }

            return true;
        }
        #endregion

        #region Reject Request
        public async Task<bool> RejectRequestAsync(string requestId, string rejectionReason)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Rejected;
            request.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // Tailor notification message based on RequestType
            string notificationTitle = "Request Rejected";
            string notificationMessage;

            switch (request.RequestType)
            {
                case RequestType.NewPlan:
                case RequestType.RecurrentPlan:
                case RequestType.RelearnPlan:
                    var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (plan != null)
                    {
                        plan.TrainingPlanStatus = TrainingPlanStatus.Rejected;
                        plan.ApproveByUserId = null; // Clear approval details
                        plan.ApproveDate = null;

                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);

                        // ❌ Reject all associated courses
                        var courses = await _courseRepository.GetCoursesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var course in courses)
                        {
                            course.Status = CourseStatus.Rejected;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);
                        }

                        // ❌ Reject all associated schedules
                        var schedules = await _trainingScheduleRepository.GetSchedulesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var schedule in schedules)
                        {
                            schedule.Status = ScheduleStatus.Canceled; // Use Canceled instead of Rejected (if applicable)
                            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
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
                    notificationMessage = $"Your request to update (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                    break;
                case RequestType.Delete:
                    notificationMessage = $"Your request to delete (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                    break;
                case RequestType.CandidateImport:
                    notificationMessage = $"Your candidate import request has been rejected. Reason: {rejectionReason}";
                    break;
                case RequestType.AssignTrainee:
                    notificationMessage = $"Your request to assign trainee import has been rejected. Reason:{rejectionReason}";
                    break;
                case RequestType.AddTraineeAssign:
                    notificationMessage = $"Your request to assign a trainee has been rejected. Reason: {rejectionReason}";
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

            return true;
        }
        #endregion
    }
}
