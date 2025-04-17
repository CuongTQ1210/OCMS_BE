using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync();
        Task<DepartmentModel> GetDepartmentByIdAsync(string departmentId);
        Task<DepartmentModel> UpdateDepartmentAsync(string departmentId, DepartmentUpdateDTO dto);
        Task<bool> DeleteDepartmentAsync(string departmentId);
        Task<DepartmentModel> CreateDepartmentAsync(DepartmentCreateDTO dto);
        Task<bool> RemoveUserFromDepartmentAsync(string userId);

        Task<bool> AssignUserToDepartmentAsync(string userId, string departmentId);
    }

}
