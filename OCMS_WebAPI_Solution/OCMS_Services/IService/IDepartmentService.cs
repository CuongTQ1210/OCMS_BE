using OCMS_BOs.Entities;
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
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    }
}
