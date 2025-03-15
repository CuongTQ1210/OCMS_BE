using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class RequestModel
    {
        
            public string RequestId { get; set; }
            public string RequestById { get; set; }
            public string? RequestEntityId { get; set; }
            public string RequestType { get; set; }
            public DateTime RequestDate { get; set; } = DateTime.Now;
            public string Description { get; set; }
            public string Notes { get; set; }

            public string Status { get; set; }
            public string? ApprovedById { get; set; } 
            public DateTime? ApprovedDate { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
