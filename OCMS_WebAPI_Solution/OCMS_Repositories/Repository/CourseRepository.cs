using OCMS_BOs.Entities;
using OCMS_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OCMS_Repositories.Repository
{
    public class CourseRepository : GenericRepository<Course>
    {
        private readonly OCMSDbContext _context;

        public CourseRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Courses.AnyAsync(c => c.CourseId == id);
        }
    }
}
