using OCMS_BOs.Entities;
using OCMS_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OCMS_Repositories.IRepository;
using System.Linq.Expressions;

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

        public async Task<IEnumerable<Course>> GetCoursesByTrainingPlanIdAsync(string trainingPlanId)
        {
            return await _context.Courses
                .Where(c => c.TrainingPlanId == trainingPlanId)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseWithDetailsAsync(string courseId)
        {
            return await _context.Courses
                .Include(c => c.CourseSubjectSpecialties)
                    .ThenInclude(s => s.Instructors)
                .Include(c => c.CourseSubjectSpecialties)
                    .ThenInclude(s => s.Schedules)
                .Include(c => c.CourseSubjectSpecialties)
                    .ThenInclude(s => s.Trainees)
                .Include(c => c.CourseSubjectSpecialties)
                    .ThenInclude(s => s.Subject)
                .Include(c => c.CourseSubjectSpecialties)
                    .ThenInclude(s => s.Specialty)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetAllWithIncludesAsync(Func<IQueryable<Course>, IQueryable<Course>> includes)
        {
            var query = _context.Set<Course>().AsQueryable();
            query = includes(query);
            return await query.ToListAsync();
        }

        public async Task<Course> GetWithIncludesAsync(Expression<Func<Course, bool>> predicate, Func<IQueryable<Course>, IQueryable<Course>> includes)
        {
            var query = _context.Set<Course>().AsQueryable();
            query = includes(query);
            return await query.FirstOrDefaultAsync(predicate);
        }
    }
}
