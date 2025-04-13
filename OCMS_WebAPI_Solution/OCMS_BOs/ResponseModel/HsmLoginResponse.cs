using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class HsmLoginResponse
    {
        public string Jsonrpc { get; set; }
        public string Id { get; set; }
        public HsmLoginResult Result { get; set; }
    }
}
