using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class UpdateSpecialtyResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Specialty updated successfully";
        public SpecialtyModel Data { get; set; }
    }
}
