using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ITrainingPlanService
    {
        Task<TrainingPlanModel> CreateTrainingPlanAsync(TrainingPlanDTO dto, string createUserId);
        Task<IEnumerable<TrainingPlanModel>> GetAllTrainingPlansAsync();
        Task<TrainingPlanModel> GetTrainingPlanByIdAsync(string id);
        Task<bool> DeleteTrainingPlanAsync(string id);
        Task<TrainingPlanModel> UpdateTrainingPlanAsync(string id, TrainingPlanDTO dto, string updateUserId);

        Task<List<TrainingPlanModel>> GetTrainingPlansByTraineeIdAsync(string traineeId);
    }
}
