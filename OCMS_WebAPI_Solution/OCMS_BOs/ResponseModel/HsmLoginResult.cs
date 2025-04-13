using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class HsmLoginResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }
}
