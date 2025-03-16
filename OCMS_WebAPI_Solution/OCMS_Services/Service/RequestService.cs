using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class RequestService : IRequestService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        public RequestService(UnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userRepository = userRepository;
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
                newRequest.RequestType == RequestType.RelearnPlan)
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
                    return await _unitOfWork.TrainingPlanRepository.ExistsAsync(tp => tp.PlanId == entityId);

                case RequestType.CreateNew:
                case RequestType.CreateRecurrent:
                case RequestType.CreateRelearn:
                    return await _unitOfWork.CourseRepository.ExistsAsync(c => c.CourseId == entityId);

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

            _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
            request.RequestUserId,
                "Request Approved",
                $"Your request ({request.RequestType}) has been approved.",
                "Request"
                );

            if (request.RequestType == RequestType.CandidateImport)
            {
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

            _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();


            // 🚀 Send notification to request creator
            await _notificationService.SendNotificationAsync(
                request.RequestUserId,
                "Request Rejected",
                $"Your request ({request.RequestType}) has been rejected. Reason: {rejectionReason}",
                "Request"
            );

            if (request.RequestType == RequestType.CandidateImport)
            {
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
