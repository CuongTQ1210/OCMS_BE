using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class ExternalCertificateRepository : GenericRepository<ExternalCertificate>, IExternalCertificateRepository
    {
        private readonly OCMSDbContext _context;
        public ExternalCertificateRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExternalCertificate>> GetExternalCertificatesByCandidateIdAsync(string candidateId)
        {
            return await _context.ExternalCertificates.Where(ec => ec.CandidateId == candidateId).ToListAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var certificate = await _context.ExternalCertificates.FindAsync(id);
            if (certificate != null)
            {
                _context.ExternalCertificates.Remove(certificate);
            }
        }
    }
}
