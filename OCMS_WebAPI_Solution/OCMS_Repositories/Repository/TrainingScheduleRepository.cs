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
    public class TrainingScheduleRepository : GenericRepository<TrainingSchedule>, ITrainingScheduleRepository
    {
        private readonly OCMSDbContext _context;
        public TrainingScheduleRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.TrainingSchedules.AnyAsync(tp => tp.ScheduleID == id);
        }
        public async Task<IEnumerable<TrainingSchedule>> GetSchedulesByCourseIdAsync(string courseId)
        {
            return await _context.TrainingSchedules
                .Where(s => s.ClassSubject.ClassId == courseId) 
                .Include(s => s.ClassSubject)
                    .ThenInclude(cs => cs.Class)
                .ToListAsync();
        }
        public async Task<List<TraineeAssign>> GetTraineeAssignmentsWithSchedulesAsync(string traineeId)
        {
            return await _context.TraineeAssignments
                .Where(ta => ta.TraineeId == traineeId)
                .Include(ta => ta.ClassSubject)
                    .ThenInclude(cs => cs.Subject)
                .Include(ta => ta.ClassSubject)
                    .ThenInclude(cs => cs.Schedules)
                .ToListAsync();
        }
        public async Task<List<TrainingSchedule>> GetSchedulesByClassSubjectIdAsync(string classSubjectId)
        {
            return await _context.TrainingSchedules
                .Where(s => s.ClassSubjectId == classSubjectId)
                .ToListAsync();
        }
    }
}
