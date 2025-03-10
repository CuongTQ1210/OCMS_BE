using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TrainingListDetail
    {
        [Key]
        public string ListDetailId { get; set; }
        [ForeignKey("TrainingList")]
        public string TrainingListId { get; set; }
        public TrainingList  TrainingList { get; set; }
        [ForeignKey("User")]
        public string PersonId   { get; set; }
        public User Person { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string Notes { get; set; }

    }
}
