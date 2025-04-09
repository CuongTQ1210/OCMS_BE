using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IGradeService
    {
        Task<IEnumerable<GradeModel>> GetAllAsync();
        Task<GradeModel> GetByIdAsync(string id);
        Task<string> CreateAsync(GradeDTO dto, string gradedByUserId);
        Task<bool> UpdateAsync(string id, GradeDTO dto);
        Task<bool> DeleteAsync(string id);
    }
}
