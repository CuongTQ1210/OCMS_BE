using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class UserModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int RoleId { get; set; }
        public string DepartmentId { get; set; }
        public string SpecialtyId { get; set; }
        public string RoleName { get; set; }
        public bool IsAssign { get; set; }

        public string AvatarUrl { get; set; }
        public string AvatarUrlWithSas { get; set; }

    }
}
