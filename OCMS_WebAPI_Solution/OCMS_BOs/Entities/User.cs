using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        // Foreign key for Role
        public int RoleId { get; set; }

        // Navigation property for Role
        public Role Role { get; set; }
    }
}
