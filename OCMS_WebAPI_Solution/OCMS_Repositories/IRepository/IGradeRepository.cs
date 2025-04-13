using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface IGradeRepository
    {
        Task<IEnumerable<Grade>> GetGradesByTraineeAssignIdAsync(string traineeAssignId);
        Task<IEnumerable<Grade>> GetGradesByCourseIdAsync(string courseId);
    }
}
