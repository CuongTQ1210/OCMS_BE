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
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        private readonly OCMSDbContext _context;
        public DepartmentRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Department>> GetDepartmentsBySpecialtyIdAsync(string specialtyId)
        {
            return await _context.Departments
                .Where(d => d.SpecialtyId == specialtyId)
                .ToListAsync();
        }
    }
}
