using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectModel>> GetAllSubjectsAsync();
        Task<SubjectModel> GetSubjectByIdAsync(string subjectId);
        Task<SubjectModel> CreateSubjectAsync(SubjectDTO dto, string createdByUserId);
        Task<SubjectModel> UpdateSubjectAsync(string subjectId, SubjectDTO dto);
        Task<bool> DeleteSubjectAsync(string subjectId);
    }
}
