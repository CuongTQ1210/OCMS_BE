using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class SpecialtyModel
    {
        public string SpecialtyId { get; set; }
        public string SpecialtyName { get; set; }
        public string Description { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ParentSpecialtyId { get; set; }
        public int Status { get; set; }
    }
}
