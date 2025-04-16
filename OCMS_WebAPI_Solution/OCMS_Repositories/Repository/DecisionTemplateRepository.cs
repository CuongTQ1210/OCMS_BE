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
    public class DecisionTemplateRepository : GenericRepository<DecisionTemplate>, IDecisionTemplateRepository
    {
        private readonly OCMSDbContext _context;
        public DecisionTemplateRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
