using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class RequestDTO
    {
        [Required]
        public RequestType RequestType { get; set; }
        public string? RequestEntityId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Notes { get; set; }
    }
}
