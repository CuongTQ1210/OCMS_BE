using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class RenewalEventModel
    {
        public DateTime RenewalDate { get; set; }
        public DateTime PreviousExpirationDate { get; set; }
        public DateTime NewExpirationDate { get; set; }
        public string RenewedByUserId { get; set; }
        public string RenewedByUserName { get; set; }
    }
}
