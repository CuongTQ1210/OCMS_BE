using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ISubjectSpecialtyService
    {
        Task<IEnumerable<SubjectSpecialtyModel>> GetAllAsync();
        Task<SubjectSpecialtyModel> GetByIdAsync(string id);
        Task<SubjectSpecialtyModel> AddAsync(SubjectSpecialtyDTO dto);
        Task<bool> DeleteAsync(string id);
    }
} 