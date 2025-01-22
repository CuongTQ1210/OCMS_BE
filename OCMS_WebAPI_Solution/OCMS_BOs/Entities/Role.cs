using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
