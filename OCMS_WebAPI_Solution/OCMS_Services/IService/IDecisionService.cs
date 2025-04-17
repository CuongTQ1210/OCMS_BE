using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IDecisionService
    {
        Task<CreateDecisionResponse> CreateDecisionForCourseAsync(CreateDecisionDTO request, string issuebyUserId);
        Task<IEnumerable<DecisionModel>> GetAllDraftDecisionsAsync();
        Task<IEnumerable<DecisionModel>> GetAllSignDecisionsAsync();
        Task<bool> DeleteDecisionAsync(string decisionId);
    }
}
