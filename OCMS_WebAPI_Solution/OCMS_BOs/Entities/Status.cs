﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public enum AccountStatus
    {
        Active = 1,
        Deactivated = 0
    }

    public enum CertificateStatus
    {
        Active = 1,
        Expired = 2,
        Revoked = 3,
        Pending = 0
    }

    public enum CourseStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum CourseLevel
    {
        Initial = 0,
        Relearn = 2,
        Recurrent = 1,
        Professional=3
    }

    public enum RequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Updating = 3,
        Deleting = 4
    }

    public enum Progress
    {
        NotYet = 0,
        Ongoing = 1,
        Completed = 2
    }
    public enum DepartmentStatus
    {
        Active = 0,
        Inactive = 1
    }
    public enum CandidateStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
    }
    public enum VerificationStatus
    {
        Pending = 0,
        Verified = 1, Rejected = 2
    }
    public enum PlanLevel
    {
        Initial = 0,
        Recurrent = 1,
        Relearn = 2
    }

    public enum GradeStatus
    {
        Pending = -1,
        Pass = 1, 
        Fail = 0
    }
    public enum GradeComponentType
    {
        Participation,
        ProgressTest,
        Assignments,
        GroupProject,
        FinalExam,
        PracticalExam,
        FinalExamResit,
        PracticalExamResit
    }

    public enum DigitalSignatureStatus
    {
        Active = 1, Expired = 0, Revoked = 2
    }

    public enum TemplateStatus
    {
        Active = 1, Inactive = 0
    }
    public enum RequestType
    {
        NewPlan = 0, NewCourse =1, UpdateCourse=2,AssignInstructor=4,ClassSchedule,DeleteCourse = 18,Complaint = 3, CreateNew = 6, CreateRecurrent = 7, CreateRelearn = 8, CandidateImport = 9, Update = 10, Delete = 11, AssignTrainee = 12, AddTraineeAssign = 13, CertificateTemplate = 14, DecisionTemplate = 15, SignRequest = 16, Revoke =17
    }
    public enum DecisionStatus
    {
        Draft = 0, Signed = 1, Revoked = 2
    }
    public enum ReportType
    {
        ExpiredCertificate = 1, CourseResult = 2, TraineeResult = 3, PlanResult = 4
    }
    public enum ResultStatus
    {
        Draft = 0, Submitted = 1, Approved = 2, Rejected = 3
    }
    public enum ScheduleStatus
    {
        Pending = 0, Approved = 1, Incoming = 2, Canceled = 3, Completed = 4
    }
}
