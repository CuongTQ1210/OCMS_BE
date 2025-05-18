using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IClassService
    {
        Task<ClassModel> CreateClassAsync(ClassDTO dto);
        Task<ClassModel> GetClassByIdAsync(string id);
        Task<IEnumerable<ClassModel>> GetAllClassesAsync();
        Task<ClassModel> UpdateClassAsync(string id, ClassDTO dto);
        Task<bool> DeleteClassAsync(string id);
    }
}
