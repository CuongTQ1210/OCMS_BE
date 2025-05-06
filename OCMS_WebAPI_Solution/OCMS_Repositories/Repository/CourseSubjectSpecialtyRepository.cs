using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class CourseSubjectSpecialtyRepository : GenericRepository<CourseSubjectSpecialty>, ICourseSubjectSpecialtyRepository
    {
        private readonly OCMSDbContext _context;

        public CourseSubjectSpecialtyRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
