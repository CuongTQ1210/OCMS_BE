using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
namespace OCMS_Repositories.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly OCMSDbContext _context;

        public UserRepository(OCMSDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                                 .Include(u => u.Role) // Ensure Role is included in the query
                                 .Where(u => u.Username == username)
                                 .FirstOrDefaultAsync();
        }

    }
}
