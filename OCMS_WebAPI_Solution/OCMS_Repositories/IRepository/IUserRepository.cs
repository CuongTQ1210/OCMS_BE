using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCMS_BOs.Entities;
namespace OCMS_Repositories.IRepository
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    }

}
