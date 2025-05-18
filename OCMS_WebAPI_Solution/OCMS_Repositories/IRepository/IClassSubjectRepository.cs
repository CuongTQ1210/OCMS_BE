using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OCMS_Repositories.IRepository
{
    public interface IClassSubjectRepository
    {
        Task<IEnumerable<ClassSubject>> GetClassSubjectsByClassIdAsync(string classId);
        Task<IEnumerable<ClassSubject>> GetClassSubjectsBySubjectIdAsync(string subjectId);
        Task<IEnumerable<ClassSubject>> GetClassSubjectsByInstructorIdAsync(string instructorId);
        Task<ClassSubject> GetClassSubjectWithDetailsByIdAsync(string id);
    }
}
