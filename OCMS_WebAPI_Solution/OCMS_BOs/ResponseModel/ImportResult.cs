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
    }
}
