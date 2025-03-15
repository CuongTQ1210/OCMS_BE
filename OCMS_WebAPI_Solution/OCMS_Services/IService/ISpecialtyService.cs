using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ISpecialtyService
    {
        Task<IEnumerable<Specialties>> GetAllSpecialtiesAsync();
        Task<Specialties> GetSpecialtyByIdAsync(string id);
        Task<Specialties> AddSpecialtyAsync(Specialties specialty);
        Task<Specialties> UpdateSpecialtyAsync(Specialties specialty);
        Task<bool> DeleteSpecialtyAsync(string id);
    }
}
