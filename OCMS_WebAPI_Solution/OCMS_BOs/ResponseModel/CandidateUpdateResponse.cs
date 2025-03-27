using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ResponseModel
{
    public class CandidateUpdateResponse
    {
        public Candidate Candidate { get; set; } // Thông tin ứng viên đã cập nhật
        public string Message { get; set; }      // Thông báo về kết quả
        public bool Success { get; set; }        // Trạng thái thành công hay thất bại
    }
}
