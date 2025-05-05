using OCMS_BOs.Entities;
using OCMS_BOs;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OCMS_Repositories.Repository
{
    public class InstructorAssignmentRepository : GenericRepository<InstructorAssignment>, IInstructorAssignmentRepository
    {
        private readonly OCMSDbContext _context;
        public InstructorAssignmentRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.InstructorAssignments.AnyAsync(tp => tp.AssignmentId == id);
        }
        public async Task<IEnumerable<InstructorAssignment>> GetAssignmentsByTrainingPlanIdAsync(string trainingPlanId)
        {
            return await _context.InstructorAssignments
                .Where(ia => ia.CourseSubjectSpecialty.Course.TrainingPlanId == trainingPlanId)
                .Include(ia => ia.CourseSubjectSpecialty.Subject)
                .ThenInclude(sub => sub.Courses) 
                .ToListAsync();
        }

        public async Task<IEnumerable<InstructorAssignment>> GetAssignmentsByInstructorIdAsync(string instructorId)
        {
            return await _context.InstructorAssignments
                .Where(ia => ia.InstructorId == instructorId)
                .ToListAsync();
        }
    }
}
