using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class CreateUserDTO
    {
        public string FullName { get; set; }

        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }

        public string SpecialtyId { get; set; }

        public string? DepartmentId { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsAssign { get; set; } = false;
        public AccountStatus Status { get; set; } = AccountStatus.Active;
    }
}
