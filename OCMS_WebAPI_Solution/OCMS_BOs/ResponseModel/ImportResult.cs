using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class ImportResult
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; }
        public string AdditionalInfo { get; set; } // Added property to fix CS1061  
        public List<string> Warnings { get; set; } // Added property to handle warnings if needed  
    }
}
