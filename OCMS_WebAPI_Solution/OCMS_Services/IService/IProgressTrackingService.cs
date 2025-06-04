using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IProgressTrackingService
    {
        Task ScheduleCourseStatusUpdatesAsync();
        Task CheckAndUpdateCourseSubjectSpecialtyStatus(string courseSubjectSpecialtyId);
        Task CheckAndUpdateClassSubjectStatus(string classSubjectId);
        Task CheckAndUpdateCourseStatus(string courseId);
        Task CheckAndUpdateAllStatuses();
    }
}
