using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TrainingList
    {
        [Key]
        public string ListId { get; set; }
        public string ListName { get; set; }
        [ForeignKey("CreateUser")]
        public string CreateByUserId {  get; set; }
        public User CreateByUser { get; set; }

        public string Purpose { get; set; }
        public RequestStatus Status { get; set; }
        [ForeignKey("ApproveUser")]
        public string ApproveByUserId {  get; set; }
        public User ApproveByUser { get; set; }
        public DateTime ApproveDate { get; set; }
        public string Notes { get; set; }
    }
}
