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
    public class GradeRepository : GenericRepository<Grade>, IGradeRepository
    {
        private readonly OCMSDbContext _context;
        public GradeRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Grade>> GetGradesByTraineeAssignIdAsync(string traineeAssignId)
        {
            return await _context.Grades
                .Where(g => g.TraineeAssignID == traineeAssignId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Grade>> GetGradesByCourseIdAsync(string courseId)
        {
            return await _context.Grades
                .Include(g => g.TraineeAssign)
                .ThenInclude(ta => ta.Trainee)
                .Where(g => g.TraineeAssign.CourseId == courseId)
                .ToListAsync();
        }
    }
}
