using OCMS_BOs.Entities;
using OCMS_BOs.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ICandidateService
    {
        Task<IEnumerable<Candidate>> GetAllCandidates();
        Task<Candidate> GetCandidateByIdAsync(string id);
        Task<ImportResult> ImportCandidatesFromExcelAsync(Stream fileStream, string importedByUserId);
    }
}
