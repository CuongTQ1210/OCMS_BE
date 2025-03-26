using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IInstructorAssignmentService
    {
        Task<IEnumerable<InstructorAssignmentModel>> GetAllInstructorAssignmentsAsync();
        Task<InstructorAssignmentModel> GetInstructorAssignmentByIdAsync(string assignmentId);
        Task<InstructorAssignmentModel> CreateInstructorAssignmentAsync(InstructorAssignmentDTO dto, string assignByUserId);
        Task<InstructorAssignmentModel> UpdateInstructorAssignmentAsync(string assignmentId, InstructorAssignmentDTO dto);
        Task<bool> DeleteInstructorAssignmentAsync(string assignmentId);
    }
}
