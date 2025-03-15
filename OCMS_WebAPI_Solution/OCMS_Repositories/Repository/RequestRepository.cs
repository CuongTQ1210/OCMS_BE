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
    public class RequestRepository : GenericRepository<Request>, IRequestRepository 
    {
        private readonly OCMSDbContext _context;
        public RequestRepository(OCMSDbContext context) : base(context)
        {
        }

    }
}
