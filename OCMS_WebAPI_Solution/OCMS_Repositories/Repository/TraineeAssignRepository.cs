﻿using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class TraineeAssignRepository:GenericRepository<TraineeAssign>, ITraineeAssignRepository
    {
        private readonly OCMSDbContext _context;
    public TraineeAssignRepository(OCMSDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.TrainingPlans.AnyAsync(tp => tp.PlanId == id);
    }
    
    }
}
