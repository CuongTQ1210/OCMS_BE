using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Candidate
    {
        [Key]
        public string CandidateId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PersonalID { get; set; }
        public List<ExternalCertificate> ExternalCertificate { get; set; } // Path to uploaded certificate
        public CandidateStatus CandidateStatus { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("User")]
        public string ImportByUserID { get; set; }
        [ForeignKey("Specialty")]
        public string SpecialtyId { get; set; }
        public User ImportByUser {  get; set; }
        public Specialties Specialty { get; set; }
    }
}
