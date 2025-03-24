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
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
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

        public async Task<Course?> GetLastObjectIdAsync()
        {
            return await _context.Courses
                .OrderByDescending(c => c.CourseId)
                .FirstOrDefaultAsync();
        }
    }
}
