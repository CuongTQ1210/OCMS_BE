using OCMS_BOs.Entities;
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
        Task<IEnumerable<SpecialtyModel>> GetAllSpecialtiesAsync();
        Task<SpecialtyModel> GetSpecialtyByIdAsync(string id);
        Task<SpecialtyModel> AddSpecialtyAsync(SpecialtyModel specialty, string createdByUserId);
        Task<SpecialtyModel> UpdateSpecialtyAsync(string id, SpecialtyModel specialty, string updatedByUserId);
        Task<bool> DeleteSpecialtyAsync(string id);
    }
}
