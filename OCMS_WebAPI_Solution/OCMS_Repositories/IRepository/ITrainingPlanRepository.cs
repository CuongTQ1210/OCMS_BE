using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface ITrainingPlanRepository
    {
        Task<bool> ExistsAsync(string id);
        Task<TrainingPlan?> GetLastTrainingPlanAsync(string seasonCode, string year);

        Task<TrainingPlan> GetTrainingPlanWithDetailsAsync(string planId);
    }
}
