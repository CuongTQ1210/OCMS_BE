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
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        private readonly OCMSDbContext _context;

        public ClassRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
