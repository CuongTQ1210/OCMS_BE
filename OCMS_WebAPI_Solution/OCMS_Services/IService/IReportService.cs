using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IReportService
    {
        Task<(byte[] fileBytes, Report report)> ExportCourseResultReportToExcelAsync(string generateByUserId);
        Task<(byte[] fileBytes, Report report)> ExportTraineeInfoToExcelAsync(string traineeId, string generateByUserId);

        Task<(byte[] fileBytes, Report report)> GenerateExcelReport(List<ExpiredCertificateReportDto> data, string filePath, string generateByUserId);
        Task<List<ExpiredCertificateReportDto>> GetExpiredCertificatesAsync();

        Task<List<TraineeInfoReportDto>> GenerateTraineeInfoReportByTraineeIdAsync(string traineeId);
        Task<List<ReportModel>> GetSavedReportsAsync();

    }
}
