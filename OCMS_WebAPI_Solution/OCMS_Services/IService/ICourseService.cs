﻿using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ICourseService
    {
        Task<CourseModel> CreateCourseAsync(CourseDTO dto, string createdByUserId);
        Task<IEnumerable<CourseModel>> GetAllCoursesAsync();
        Task<CourseModel?> GetCourseByIdAsync(string id);
        Task<bool> DeleteCourseAsync(string id);
        Task<CourseModel> UpdateCourseAsync(string id, CourseUpdateDTO dto, string updatedByUserId);

        Task<bool> SendCourseRequestForApprove(string courseId, string createdByUserId);
        Task<ImportResult> ImportCoursesAsync(Stream excelStream, string importedByUserId);
    }
}
