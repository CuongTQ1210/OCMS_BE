using OCMS_BOs.Entities;
using OCMS_BOs;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class GradeRepository : GenericRepository<Grade>, IGradeRepository
    {
        private readonly OCMSDbContext _context;
        public GradeRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
        
    }
}
