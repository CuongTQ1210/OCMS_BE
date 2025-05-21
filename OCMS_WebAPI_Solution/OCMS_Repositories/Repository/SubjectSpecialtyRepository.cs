using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OCMS_BOs;

namespace OCMS_Repositories.Repository
{
    public class SubjectSpecialtyRepository : GenericRepository<SubjectSpecialty>, ISubjectSpecialtyRepository
    {
        private readonly DbContext _context;
        public SubjectSpecialtyRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubjectSpecialty>> GetAllWithIncludesAsync()
        {
            return await _context.Set<SubjectSpecialty>()
                .Include(ss => ss.Subject)
                .Include(ss => ss.Specialty)
                .ToListAsync();
        }

        public async Task<SubjectSpecialty> GetWithIncludesAsync(string id)
        {
            return await _context.Set<SubjectSpecialty>()
                .Include(ss => ss.Subject)
                .Include(ss => ss.Specialty)
                .FirstOrDefaultAsync(ss => ss.SubjectSpecialtyId == id);
        }
    }
} 