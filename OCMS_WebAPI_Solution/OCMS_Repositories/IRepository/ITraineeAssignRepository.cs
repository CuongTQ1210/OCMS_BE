using OCMS_BOs.Entities;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface ITraineeAssignRepository
    {
        Task<bool> ExistsAsync(string id);
        Task<TraineeAssign> GetTraineeAssignmentAsync(string courseId, string traineeId);
        Task<List<TraineeAssignModel>> GetTraineeAssignmentsByRequestIdAsync(string requestId);

        Task<List<TraineeAssignModel>> GetTraineeAssignsBySubjectIdAsync(string subjectId);
        Task<IEnumerable<TraineeAssign>> GetTraineeAssignmentsByCourseIdAsync(string courseId);
    }
}
