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
    public class CandidateRepository : GenericRepository<Candidate>, ICandidateRepository
    {
        private readonly OCMSDbContext _context;
        public CandidateRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Candidate>> GetCandidatesByImportRequestIdAsync(string importRequestId)
        {

            return await _context.Candidates
                .Where(c => c.ImportRequestId == importRequestId)
                .ToListAsync();
        }
    }
}
