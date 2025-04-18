using OCMS_BOs.RequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IReportService
    {
        Task<byte[]> ExportCourseResultReportToExcelAsync();
        Task<byte[]> ExportTraineeInfoToExcelAsync(string traineeId);

        Task GenerateExcelReport(List<ExpiredCertificateReportDto> data, string filePath);
        Task<List<ExpiredCertificateReportDto>> GetExpiredCertificatesAsync();

        Task<List<TraineeInfoReportDto>> GenerateTraineeInfoReportByTraineeIdAsync(string traineeId);

    }
}
