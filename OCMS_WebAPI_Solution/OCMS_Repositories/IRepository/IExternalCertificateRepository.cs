using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface IExternalCertificateRepository
    {
        Task<IEnumerable<ExternalCertificate>> GetExternalCertificatesByCandidateIdAsync(string candidateId);
        Task RemoveAsync(int id);
    }
}
