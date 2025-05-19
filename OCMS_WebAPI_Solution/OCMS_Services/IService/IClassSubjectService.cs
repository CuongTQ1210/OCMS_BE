using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IClassSubjectService
    {
        Task<ClassSubjectModel> CreateClassSubjectAsync(ClassSubjectDTO dto);
        Task<ClassSubjectModel> GetClassSubjectByIdAsync(string id);
        Task<IEnumerable<ClassSubjectModel>> GetAllClassSubjectsAsync();
        Task<IEnumerable<ClassSubjectModel>> GetClassSubjectsByClassIdAsync(string classId);
        Task<IEnumerable<ClassSubjectModel>> GetClassSubjectsBySubjectIdAsync(string subjectId);
        Task<ClassSubjectModel> UpdateClassSubjectAsync(string id, ClassSubjectDTO dto);
        Task<bool> DeleteClassSubjectAsync(string id);
        Task<IEnumerable<ClassSubjectModel>> GetClassSubjectsByInstructorIdAsync(string instructorId);
        Task<ClassSubjectDetailModel> GetClassSubjectDetailsByIdAsync(string id);
    }
}
