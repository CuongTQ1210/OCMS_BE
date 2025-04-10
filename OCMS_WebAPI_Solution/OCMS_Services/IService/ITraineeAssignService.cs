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
    public interface ITraineeAssignService
    {
        Task<TraineeAssignModel> CreateTraineeAssignAsync(TraineeAssignDTO dto, string createdByUserId);
        Task<ImportResult> ImportTraineeAssignmentsFromExcelAsync(Stream fileStream, string importedByUserId);
        Task<IEnumerable<TraineeAssignModel>> GetAllTraineeAssignmentsAsync();
        Task<TraineeAssignModel> UpdateTraineeAssignmentAsync(string id, TraineeAssignDTO dto);
        Task<(bool success, string message)> DeleteTraineeAssignmentAsync(string id);
        Task<TraineeAssignModel> GetTraineeAssignmentByIdAsync(string traineeAssignId);

        Task<IEnumerable<CourseModel>> GetCoursesByTraineeIdAsync(string traineeId);
        Task<List<TraineeAssignModel>> GetTraineesBySubjectIdAsync(string subjectId);
    }
}
