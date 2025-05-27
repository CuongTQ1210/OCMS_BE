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

        //public async Task<Course?> GetCourseByTrainingPlanIdAsync(string trainingPlanId)
        //{
        //    var trainingPlan = await _context.TrainingPlans
        //        .Include(tp => tp.Course)
        //        .FirstOrDefaultAsync(tp => tp.PlanId == trainingPlanId);

        //    return trainingPlan?.Course;
        //}

        public async Task<Course> GetCourseByClassIdAsync(string classId)
        {
            return await _context.Set<Class>()
                .Where(c => c.ClassId == classId)
                .Include(c => c.Course)
                    .ThenInclude(course => course.SubjectSpecialties)
                        .ThenInclude(ss => ss.Subject)
                .Include(c => c.Course)
                    .ThenInclude(course => course.CreatedByUser)
                .Select(c => c.Course)
                .FirstOrDefaultAsync();
        }

        public async Task<Course?> GetCourseWithDetailsAsync(string courseId)
        {
            var course = await _context.Courses
                .Include(c => c.SubjectSpecialties)
                    .ThenInclude(s => s.Subject)
                .Include(c => c.SubjectSpecialties)
                    .ThenInclude(s => s.Specialty)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course != null)
            {
                // Get subject IDs from the course
                var subjectIds = course.SubjectSpecialties
                    .Select(ss => ss.SubjectId)
                    .ToList();

                // Load instructors, schedules, and trainees separately using subject IDs
                var classSubjects = await _context.ClassSubjects
                    .Include(cs => cs.InstructorAssignment)
                        .ThenInclude(ia => ia.Instructor)
                    .Include(cs => cs.Schedules)
                    .Include(cs => cs.traineeAssigns)
                        .ThenInclude(ta => ta.Trainee)
                    .Where(cs => subjectIds.Contains(cs.SubjectSpecialty.SubjectId))
                    .ToListAsync();
            }

            return course;
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
