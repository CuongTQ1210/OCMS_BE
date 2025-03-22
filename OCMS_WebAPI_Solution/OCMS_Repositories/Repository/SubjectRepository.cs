using OCMS_BOs.Entities;
using OCMS_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OCMS_Repositories.IRepository;

namespace OCMS_Repositories.Repository
{
    public class SubjectRepository : GenericRepository<Subject>,ISubjectRepository
    {
        private readonly OCMSDbContext _context;

        public SubjectRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Subjects.AnyAsync(s => s.SubjectId == id);
        }
    }

}
