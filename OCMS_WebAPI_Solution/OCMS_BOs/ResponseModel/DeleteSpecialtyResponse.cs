using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class DeleteSpecialtyResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Specialty deleted successfully";
        public string SpecialtyId { get; set; }
    }
}
