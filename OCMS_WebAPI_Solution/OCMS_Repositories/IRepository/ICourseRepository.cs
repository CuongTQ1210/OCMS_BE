﻿using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        Task<IEnumerable<Course>> GetAllWithDetailsAsync();

        Task<Course?> GetByIdWithDetailsAsync(string id);
    }
}
