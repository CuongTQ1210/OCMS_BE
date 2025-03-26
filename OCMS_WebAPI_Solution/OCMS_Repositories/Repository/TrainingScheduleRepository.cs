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
    }
}
