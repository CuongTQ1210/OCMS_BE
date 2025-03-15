using Microsoft.EntityFrameworkCore;
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
    public class SpecialtyRepository : GenericRepository<Specialties>, ISpecialtyRepository
    {
        private readonly OCMSDbContext _context;

        public SpecialtyRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
