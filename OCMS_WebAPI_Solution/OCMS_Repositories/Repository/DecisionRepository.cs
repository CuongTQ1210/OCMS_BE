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
    public class DecisionRepository : GenericRepository<Decision>, IDecisionRepository
    {
        private readonly OCMSDbContext _context;
        public DecisionRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Decision>> GetDecisionsByCertificateIdAsync(string certificateId)
        {
            return await _context.Decisions
                .Where(d => d.CertificateId == certificateId)
                .Include(d => d.IssuedByUser)
                .Include(d => d.DecisionTemplate)
                .ToListAsync();
        }

        public async Task<Decision> GetLatestDecisionAsync()
        {
            return await _context.Decisions
                .OrderByDescending(d => d.IssueDate)
                .FirstOrDefaultAsync();
        }
    }
}
