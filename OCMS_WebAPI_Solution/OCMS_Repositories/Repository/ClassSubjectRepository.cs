using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class ClassSubjectRepository : GenericRepository<ClassSubject>, IClassSubjectRepository
    {
        private readonly OCMSDbContext _context;

        public ClassSubjectRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassSubject>> GetClassSubjectsByClassIdAsync(string classId)
        {
            return await _context.ClassSubjects
                .Include(cs => cs.Class)
                .Include(cs => cs.Subject)
                .Include(cs => cs.InstructorAssignment)
                .ThenInclude(ia => ia.Instructor)
                .Where(cs => cs.ClassId == classId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassSubject>> GetClassSubjectsBySubjectIdAsync(string subjectId)
        {
            return await _context.ClassSubjects
                .Include(cs => cs.Class)
                .Include(cs => cs.Subject)
                .Include(cs => cs.InstructorAssignment)
                .ThenInclude(ia => ia.Instructor)
                .Where(cs => cs.SubjectId == subjectId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassSubject>> GetClassSubjectsByInstructorIdAsync(string instructorId)
        {
            return await _context.ClassSubjects
                .Include(cs => cs.Class)
                .Include(cs => cs.Subject)
                .Include(cs => cs.InstructorAssignment)
                .ThenInclude(ia => ia.Instructor)
                .Where(cs => cs.InstructorAssignment.InstructorId == instructorId)
                .ToListAsync();
        }

        public async Task<ClassSubject> GetClassSubjectWithDetailsByIdAsync(string id)
        {
            return await _context.ClassSubjects
                .Include(cs => cs.Class)
                .Include(cs => cs.Subject)
                .Include(cs => cs.InstructorAssignment)
                .ThenInclude(ia => ia.Instructor)
                .Include(cs => cs.traineeAssigns)
                .Include(cs => cs.Schedules)
                .FirstOrDefaultAsync(cs => cs.ClassSubjectId == id);
        }
    }
}
