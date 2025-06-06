﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class User
    {
        [Key]
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

        [ForeignKey("Role")]
        public int RoleId { get; set; }

        [ForeignKey("Specialty")]
        public string? SpecialtyId { get; set; }

        [ForeignKey("Department")]
        public string? DepartmentId { get; set; }
        public bool IsAssign {  get; set; } =false;
        public AccountStatus Status { get; set; } = AccountStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public Role Role { get; set; }
        public Department? Department { get; set; }
        public Specialties? Specialty { get; set; }

        public string? AvatarUrl { get; set; } = string.Empty;
    }
}
