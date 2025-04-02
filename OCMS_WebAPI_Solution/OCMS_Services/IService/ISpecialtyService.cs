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
    public interface ISpecialtyService
    {
        Task<SpecialtyModel> AddSpecialtyAsync(CreateSpecialtyDTO model, string createdByUserId);

        Task<SpecialtyModel> UpdateSpecialtyAsync(string id, UpdateSpecialtyDTO model, string updatedByUserId);

        Task<bool> DeleteSpecialtyAsync(string id);

        Task<IEnumerable<SpecialtyModel>> GetAllSpecialtiesAsync();

        Task<IEnumerable<SpecialtyTreeModel>> GetSpecialtyTreeAsync();

        Task<SpecialtyModel> GetSpecialtyByIdAsync(string id);

        Task<string> GenerateSpecialtyId(string specialtyName, string parentSpecialtyId = null);
    }
}
