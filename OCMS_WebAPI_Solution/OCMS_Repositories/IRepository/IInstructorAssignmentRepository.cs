using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface IInstructorAssignmentRepository
    {
        Task<bool> ExistsAsync(string id);
        Task<IEnumerable<InstructorAssignment>> GetAssignmentsByTrainingPlanIdAsync(string trainingPlanId);
    }
}
