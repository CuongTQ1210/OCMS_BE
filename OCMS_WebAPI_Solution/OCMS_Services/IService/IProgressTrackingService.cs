using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IProgressTrackingService
    {
        Task CheckAndUpdateCourseSubjectSpecialtyStatus(string courseSubjectSpecialtyId);
        Task CheckAndUpdateCourseStatus(string courseId);
        Task CheckAndUpdateTrainingPlanStatus(string planId);
        Task CheckAndUpdateAllStatuses();
    }
}
