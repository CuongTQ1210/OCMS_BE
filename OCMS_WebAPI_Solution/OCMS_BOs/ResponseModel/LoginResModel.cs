using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class LoginResModel
    {
        public string UserID { get; set; }
        public string Token { get; set; }
        public List<string> Roles { get; set; }
    }
}
