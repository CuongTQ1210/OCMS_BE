using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public enum AccountStatus
    {
        Active,
        Deactivated
    }


    public enum CertificateStatus
    {
        Active,
        Expired,
        Revoked,
        Returned
    }

    public enum CourseStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum CourseType
    {
        Initial,
        Relearn,
        Recurrent
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum CourseParticipantStatus
    {
        Active,
        Withdrawn
    }

    public enum  Progress
    {
        Ongoing,
        Completed
    }
}
