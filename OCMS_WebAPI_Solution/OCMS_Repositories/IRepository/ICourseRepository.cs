﻿using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface ICourseRepository 
    {
        Task<bool> ExistsAsync(string id);
        Task<Course?> GetLastObjectIdAsync();

        Task<IEnumerable<Course>> GetCoursesByTrainingPlanIdAsync(string trainingPlanId);

        Task<Course?> GetCourseWithDetailsAsync(string courseId);
        Task<IEnumerable<Course>> GetAllWithIncludesAsync(Func<IQueryable<Course>, IQueryable<Course>> includes);
        Task<Course> GetWithIncludesAsync(Expression<Func<Course, bool>> predicate, Func<IQueryable<Course>, IQueryable<Course>> includes);
    }
}
