using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ICourseSubjectSpecialtyService
    {
        Task<CourseSubjectSpecialtyModel> CreateCourseSubjectSpecialtyAsync(CourseSubjectSpecialtyDTO dto, string createdByUserId);
        Task<bool> DeleteCourseSubjectSpecialtyAsync(string id, string deletedByUserId);
        Task<bool> DeleteSubjectsbyCourseIdandSpecialtyId(DeleteAllSubjectInCourseSpecialty dto, string deletedByUserId);
        Task<List<SubjectModel>> GetSubjectsByCourseIdAndSpecialtyIdAsync(string courseId, string specialtyId);
        Task<List<CourseSubjectSpecialtyModel>> GetAllCourseSubjectSpecialtiesAsync();
    }
}
