using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Report
    {
        [Key]
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public ReportType ReportType { get; set; }
        [ForeignKey("GenerateUser")]
        public string GenerateByUserId { get; set; }
        public User GenerateByUser { get; set; }
        public DateTime GenerateDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Content { get; set; }
        public string Format {  get; set; }
        public string FileUrl { get; set; }
    }
}
