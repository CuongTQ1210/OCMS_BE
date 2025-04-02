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
    public class SpecialtyRepository : GenericRepository<Specialties>, ISpecialtyRepository
    {
        private readonly OCMSDbContext _context;

        public SpecialtyRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> isExistSpecialty(string? name=null, string? id = null)
        {
            if (name != null) { 
            return await _context.Specialties.AnyAsync(s => s.SpecialtyName.Trim().ToLower() == name.Trim().ToLower());
                
            }
            if (id != null)
            {
                return await _context.Specialties.AnyAsync(s => s.SpecialtyId.Trim().ToLower() == id.Trim().ToLower());
            }
            return false;
        }
    }
}
