using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class GetSpecialtyTreeResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Specialty tree retrieved successfully";
        public List<SpecialtyTreeModel> Data { get; set; } = new List<SpecialtyTreeModel>();
    }
}
