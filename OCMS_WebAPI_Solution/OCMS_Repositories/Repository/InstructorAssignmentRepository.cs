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
        
        public async Task<IEnumerable<InstructorAssignment>> GetAssignmentsByCourseIdAsync(string courseId)
        {
            // Get ClassSubjects for this course
            var classSubjects = await _context.ClassSubjects
                .Where(cs => cs.ClassId == courseId)
                .Select(cs => cs.SubjectId)
                .ToListAsync();

            // Get instructor assignments for these subjects
            return await _context.InstructorAssignments
                .Where(ia => classSubjects.Contains(ia.SubjectId))
                .Include(ia => ia.Subject)
                .ToListAsync();
        }

        public async Task<IEnumerable<InstructorAssignment>> GetAssignmentsByInstructorIdAsync(string instructorId)
        {
            return await _context.InstructorAssignments
                .Where(ia => ia.InstructorId == instructorId)
                .Include(ia => ia.Subject)
                .ToListAsync();
        }
    }
}
