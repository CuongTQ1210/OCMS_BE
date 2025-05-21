using OCMS_BOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface ISubjectSpecialtyRepository
    {
        Task<IEnumerable<SubjectSpecialty>> GetAllWithIncludesAsync();
        Task<SubjectSpecialty> GetWithIncludesAsync(string id);
    }
} 