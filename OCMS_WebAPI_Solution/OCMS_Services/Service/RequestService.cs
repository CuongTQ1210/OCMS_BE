using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
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

        public RequestService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
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

            return newRequest;
        }

        public async Task<IEnumerable<RequestModel>> GetAllRequestsAsync()
        {
            var requests = await _unitOfWork.RequestRepository.GetAllAsync(); // Remove includeProperties
            

            return _mapper.Map<IEnumerable<RequestModel>>(requests);
        }

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
        public async Task<bool> DeleteRequestAsync(string requestId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return false;

            await _unitOfWork.RequestRepository.DeleteAsync(requestId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

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
            return true;
        }

        public async Task<bool> RejectRequestAsync(string requestId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Rejected;
            request.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
