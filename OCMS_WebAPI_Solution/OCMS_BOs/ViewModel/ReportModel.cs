using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class ReportModel
    {
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public string ReportType { get; set; } // Convert enum to string for display
        public string GenerateByUserId { get; set; }
        public string GenerateByUserName { get; set; } // Display the user's name instead of just ID
        public string GenerateDate { get; set; } // Formatted date string
        public string StartDate { get; set; } // Formatted date string
        public string EndDate { get; set; } // Formatted date string
        public string Content { get; set; } // Summary or metadata
        public string Format { get; set; }
        public string FileUrl { get; set; } // URL to download the file
    }
}
