using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IRequestService
    {
        Task<Request> CreateRequestAsync(RequestDTO requestDto, string userId);
        
        Task<IEnumerable<RequestModel>> GetAllRequestsAsync();
        Task<RequestModel> GetRequestByIdAsync(string requestId);
        Task<bool> DeleteRequestAsync(string requestId);
        Task<bool> ApproveRequestAsync(string requestId, string approvedByUserId);
        Task<bool> RejectRequestAsync(string requestId, string rejectionReason);
        Task<List<RequestModel>> GetRequestsForEducationOfficerAsync();
    }
}
