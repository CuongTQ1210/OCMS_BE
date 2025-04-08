using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface ITrainingScheduleRepository
    {
        Task<bool> ExistsAsync(string id);
        Task<IEnumerable<TrainingSchedule>> GetSchedulesByTrainingPlanIdAsync(string trainingPlanId);

        Task<List<TraineeAssign>> GetTraineeAssignmentsWithSchedulesAsync(string traineeId);
    }
}
