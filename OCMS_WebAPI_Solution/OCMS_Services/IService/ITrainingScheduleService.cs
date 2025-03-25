using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ITrainingScheduleService
    {
        Task<IEnumerable<TrainingScheduleModel>> GetAllTrainingSchedulesAsync();
        Task<TrainingScheduleModel> GetTrainingScheduleByIdAsync(string scheduleId);
        Task<TrainingScheduleModel> CreateTrainingScheduleAsync(TrainingScheduleDTO dto, string createdByUserId);
        Task<TrainingScheduleModel> UpdateTrainingScheduleAsync(string scheduleId, TrainingScheduleDTO dto);
        Task<bool> DeleteTrainingScheduleAsync(string scheduleId);
    }
}
