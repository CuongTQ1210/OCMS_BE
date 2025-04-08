using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class TrainingPlanRepository: GenericRepository<TrainingPlan>, ITrainingPlanRepository
    {
        private readonly OCMSDbContext _context;
        public TrainingPlanRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.TrainingPlans.AnyAsync(tp => tp.PlanId == id);
        }
        public async Task<TrainingPlan?> GetLastTrainingPlanAsync(string specialtyId, string seasonCode, string year, PlanLevel planLevel)
        {
            return await _context.Set<TrainingPlan>()
                .Where(tp => tp.SpecialtyId == specialtyId &&
                             tp.PlanId.StartsWith($"{specialtyId}-{seasonCode}{year}") &&
                             tp.PlanLevel == planLevel)
                .OrderByDescending(tp => tp.PlanId)
                .FirstOrDefaultAsync();
        }

        public async Task<TrainingPlan> GetTrainingPlanWithDetailsAsync(string planId)
        {
            return await _context.TrainingPlans
                .Include(p => p.CreateByUser)
                .Include(p => p.Specialty)
                .Include(p => p.Courses)
                    .ThenInclude(c => c.Trainees)
                .Include(p => p.Courses)
                    .ThenInclude(c => c.Subjects)
                        .ThenInclude(s => s.Instructors)
                .Include(p => p.Courses)
                    .ThenInclude(c => c.Subjects)
                        .ThenInclude(s => s.Schedules)
                .FirstOrDefaultAsync(p => p.PlanId == planId);
        }
    }
}
