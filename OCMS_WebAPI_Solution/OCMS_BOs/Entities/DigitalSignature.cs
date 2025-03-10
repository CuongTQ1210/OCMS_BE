using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class DigitalSignature
    {
        [Key]
        public string SignatureID { get; set; }
        [ForeignKey("User")]
        public string UserId {  get; set; }
        public User User { get; set; }
        public string PublicKey { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
        
        public DigitalSignatureStatus Status { get; set; }
        public string Provider {  get; set; }
        public string CertificateChain { get; set; }
    }
}
