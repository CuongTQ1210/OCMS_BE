using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class UserCreateDTO
    {
        public string UserId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public int RoleId { get; set; }

        public string SpecialtyId { get; set; }

        public string? DepartmentId { get; set; }
    }
}
