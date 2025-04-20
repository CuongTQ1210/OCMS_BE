using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Helper
{
    public class MappingHelper : Profile
    {
        public MappingHelper()
        {
            CreateMap<User, UserModel>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl)) // ✅ Added mapping
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ReverseMap();
            CreateMap<UserUpdateDTO, User>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
            CreateMap<CreateUserDTO, User>();

            CreateMap<CandidateUpdateDTO, Candidate>()
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore()) // Bỏ qua CandidateId
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Bỏ qua CreatedAt
                .ForMember(dest => dest.ImportRequestId, opt => opt.Ignore()) // Bỏ qua ImportRequestId
                .ForMember(dest => dest.ImportByUserID, opt => opt.Ignore()) // Bỏ qua ImportByUserID
                .ForMember(dest => dest.CandidateStatus, opt => opt.Ignore()) // Bỏ qua CandidateStatus
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Bỏ qua UpdatedAt

            CreateMap<ExternalCertificateCreateDTO, ExternalCertificate>()
            .ForMember(dest => dest.VerifyByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.VerifyByUser, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Candidate, opt => opt.Ignore())
            .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => VerificationStatus.Pending))
            .ForMember(dest => dest.VerifyDate, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.CertificateFileURL, opt => opt.Ignore()) // Assuming you upload this separately
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now));

            CreateMap<ExternalCertificateUpdateDTO, ExternalCertificate>()
    .ForMember(dest => dest.VerifyByUserId, opt => opt.Ignore())
    .ForMember(dest => dest.VerifyByUser, opt => opt.Ignore())
    .ForMember(dest => dest.UserId, opt => opt.Ignore())
    .ForMember(dest => dest.User, opt => opt.Ignore())
    .ForMember(dest => dest.Candidate, opt => opt.Ignore())
    .ForMember(dest => dest.VerificationStatus, opt => opt.Ignore()) 
    .ForMember(dest => dest.VerifyDate, opt => opt.Ignore()) 
    .ForMember(dest => dest.CertificateFileURL, opt => opt.Ignore()) 
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); 
            CreateMap<Specialties, SpecialtyModel>();
            CreateMap<SpecialtyModel, Specialties>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore());

            CreateMap<CreateSpecialtyDTO, Specialties>()
                .ForMember(dest => dest.SpecialtyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUserId, opt => opt.Ignore());

            // UpdateSpecialtyDTO to Specialties mapping
            CreateMap<UpdateSpecialtyDTO, Specialties>()
                .ForMember(dest => dest.SpecialtyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUserId, opt => opt.Ignore());
            //Department 
            CreateMap<Department, DepartmentModel>()
            .ForMember(dest => dest.ManagerUserId, opt => opt.MapFrom(src => src.ManagerUserId))
            .ForMember(dest => dest.SpecialtyId, opt => opt.MapFrom(src => src.SpecialtyId));

            CreateMap<DepartmentModel, Department>()
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.Specialty, opt => opt.Ignore());


            CreateMap<Specialties, SpecialtyTreeModel>()
                .ForMember(dest => dest.Children, opt => opt.Ignore());

            CreateMap<Request, ViewModel.RequestModel>()
                .ForMember(dest => dest.RequestById, opt => opt.MapFrom(src => src.RequestUserId))
                .ForMember(dest => dest.ActionByUserId, opt => opt.MapFrom(src => src.ApprovedBy))
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(src => src.RequestType.ToString())) // Convert Enum to String
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // Convert Enum to String
            .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src => src.ApprovedDate));
            CreateMap<ViewModel.RequestModel, Request>()
                .ForMember(dest => dest.RequestUserId, opt => opt.MapFrom(src => src.RequestById))
                .ForMember(dest => dest.ApprovedBy, opt => opt.MapFrom(src => src.ActionByUserId))
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(src => Enum.Parse<RequestType>(src.RequestType))) // Convert String to Enum
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<RequestStatus>(src.Status))) // Convert String to Enum
                         .ForMember(dest => dest.ApprovedDate, opt => opt.MapFrom(src => src.ActionDate));                
            // Notification Mapping
            CreateMap<Notification, NotificationModel>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.NotificationType, opt => opt.MapFrom(src => src.NotificationType))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead));

            CreateMap<NotificationModel, Notification>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.NotificationType, opt => opt.MapFrom(src => src.NotificationType))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false));

            CreateMap<TrainingPlan, TrainingPlanModel>()
                .ForMember(dest => dest.TrainingPlanStatus, opt => opt.MapFrom(src => src.TrainingPlanStatus.ToString()))
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.Courses))
                .ReverseMap();
            CreateMap<TrainingPlanDTO, TrainingPlan>();
            CreateMap<TrainingPlan, TrainingPlanDTO>();
            // Course Mapping
            CreateMap<Course, CourseModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CourseLevel, opt => opt.MapFrom(src => src.CourseLevel.ToString()))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Progress.ToString()))
                .ForMember(dest => dest.Trainees, opt => opt.MapFrom(src => src.Trainees))
                .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects))
                .ReverseMap();
            CreateMap<CourseDTO, Course>();
            CreateMap<CourseUpdateDTO, Course>()
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Progress, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.ApproveByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.ApproveByUser, opt => opt.Ignore())
            .ForMember(dest => dest.TrainingPlan, opt => opt.Ignore())
            .ForMember(dest => dest.Subjects, opt => opt.Ignore())
            .ForMember(dest => dest.Trainees, opt => opt.Ignore());
            CreateMap<Course, CourseDTO>();
            // Subject Mapping
            CreateMap<Subject, SubjectModel>()
                .ForMember(dest => dest.Instructors, opt => opt.MapFrom(src => src.Instructors))
                .ForMember(dest => dest.trainingSchedules, opt => opt.MapFrom(src => src.Schedules))
                .ReverseMap();
            CreateMap<SubjectDTO, Subject>();
            CreateMap<Subject, SubjectDTO>();
            // Trainee Assignment Mapping
            // Mapping TraineeAssign → TraineeAssignModel (ViewModel for displaying data)
            CreateMap<TraineeAssign, TraineeAssignModel>()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => src.RequestStatus.ToString())) // Convert Enum to String
                .ReverseMap()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => Enum.Parse<RequestStatus>(src.RequestStatus))) // Convert String to Enum
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(src => src.AssignDate == default ? DateTime.Now : src.AssignDate)) // Default AssignDate
                .ForMember(dest => dest.ApprovalDate, opt => opt.MapFrom(src => src.ApprovalDate == default ? null : src.ApprovalDate)); // Keep null if not approved

            // Mapping TraineeAssignDTO → TraineeAssign (Used for Creating Assignments)
            CreateMap<TraineeAssignDTO, TraineeAssign>()
                .ForMember(dest => dest.TraineeAssignId, opt => opt.Ignore()) // Ignore ID, auto-generated
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Pending)) // Default to Pending
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(_ => DateTime.Now)) // Set AssignDate to now
                .ForMember(dest => dest.ApprovalDate, opt => opt.Ignore()) // ApprovalDate ignored during creation
                .ForMember(dest => dest.ApproveByUserId, opt => opt.Ignore()) // Approval user not set at creation
                .ForMember(dest => dest.RequestId, opt => opt.Ignore()) // Request will be assigned later
                .ForMember(dest => dest.Request, opt => opt.Ignore()); // Ignore navigation property
            CreateMap<TraineeAssign, TraineeAssignDTO>();
            // Instructor Assignment Mapping
            CreateMap<InstructorAssignmentDTO, InstructorAssignment>()
    .ForMember(dest => dest.AssignmentId, opt => opt.Ignore()) // AssignmentId should be generated, not mapped
    .ForMember(dest => dest.AssignByUserId, opt => opt.Ignore()) // AssignByUserId is set elsewhere
    .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(src => DateTime.Now)) // Set default assign date
    .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => "Pending")) // Default status if needed
    .ReverseMap();

            CreateMap<InstructorAssignment, InstructorAssignmentModel>()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => src.RequestStatus.ToString())) // Ensure status is a string
                .ReverseMap();


            // Mapping from TrainingSchedule to TrainingScheduleModel
            CreateMap<TrainingSchedule, TrainingScheduleModel>()
    .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.SubjectName))
    .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.StartDateTime))
    .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.EndDateTime))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src =>
        src.DaysOfWeek != null
            ? string.Join(", ", src.DaysOfWeek.Select(d => d.ToString()))
            : ""))
    .ForMember(dest => dest.SubjectPeriod, opt => opt.MapFrom(src => src.SubjectPeriod))
    .ReverseMap()
    .ForMember(dest => dest.DaysOfWeek, opt => opt.Ignore()); // still ignoring reverse mapping for DaysOfWeek

            // Mapping from TrainingScheduleDTO to TrainingSchedule
            CreateMap<TrainingScheduleDTO, TrainingSchedule>()
    .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src =>
        src.DaysOfWeek != null
            ? src.DaysOfWeek.Select(d => (DayOfWeek)d).ToList()
            : new List<DayOfWeek>()))
    .ForMember(dest => dest.SubjectPeriod, opt => opt.MapFrom(src => src.SubjectPeriod))
    .ReverseMap()
    .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.StartDateTime))
    .ForMember(dest => dest.EndDay, opt => opt.MapFrom(src => src.EndDateTime))
    .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src =>
        src.DaysOfWeek != null
            ? src.DaysOfWeek.Select(d => (int)d).ToList()
            : new List<int>()))
    .ForMember(dest => dest.SubjectPeriod, opt => opt.MapFrom(src => src.SubjectPeriod));
            CreateMap<CourseParticipant, CourseParticipantModel>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))

                .ReverseMap();

            CreateMap<CourseParticipantDto, CourseParticipant>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<CourseParticipantStatus>(src.Status)))
                .ReverseMap();
            CreateMap<Grade, GradeModel>()
    .ForMember(dest => dest.GradeStatus, opt => opt.MapFrom(src => src.gradeStatus.ToString()));

            CreateMap<GradeDTO, Grade>()
                .ForMember(dest => dest.GradeId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalScore, opt => opt.Ignore()) // Calculated separately
                .ForMember(dest => dest.gradeStatus, opt => opt.Ignore()) // Determined based on TotalScore
                .ForMember(dest => dest.GradedByInstructorId, opt => opt.Ignore())
                .ForMember(dest => dest.GradedByInstructor, opt => opt.Ignore())
                .ForMember(dest => dest.EvaluationDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(_ => DateTime.Now));

            CreateMap<Grade, GradeDTO>();

            CreateMap<ExternalCertificateModel, ExternalCertificate>()
    .ForMember(dest => dest.ExternalCertificateId, opt => opt.Ignore()) // ID is likely auto-generated
    .ForMember(dest => dest.CertificateCode, opt => opt.MapFrom(src => src.CertificateCode))
    .ForMember(dest => dest.CertificateName, opt => opt.MapFrom(src => src.CertificateName))
    .ForMember(dest => dest.IssuingOrganization, opt => opt.MapFrom(src => src.CertificateProvider))
    .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.CandidateId))
    .ForMember(dest => dest.CertificateFileURL, opt => opt.Ignore()) // You’ll set this after upload
    .ForMember(dest => dest.UserId, opt => opt.Ignore())
    .ForMember(dest => dest.User, opt => opt.Ignore())
    .ForMember(dest => dest.Candidate, opt => opt.Ignore())
    .ForMember(dest => dest.VerifyByUserId, opt => opt.Ignore())
    .ForMember(dest => dest.VerifyByUser, opt => opt.Ignore())
    .ForMember(dest => dest.VerifyDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
    .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(_ => VerificationStatus.Pending))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<ExternalCertificate, ExternalCertificateModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalCertificateId.ToString()))
                .ForMember(dest => dest.CertificateCode, opt => opt.MapFrom(src => src.CertificateCode))
                .ForMember(dest => dest.CertificateName, opt => opt.MapFrom(src => src.CertificateName))
                .ForMember(dest => dest.CertificateProvider, opt => opt.MapFrom(src => src.IssuingOrganization))
                .ForMember(dest => dest.CertificateFileURL, opt => opt.MapFrom(src => src.CertificateFileURL))
                .ForMember(dest => dest.CertificateFileURLWithSas, opt => opt.Ignore()); // This is for SAS generation


            CreateMap<CreateCertificateTemplateDTO, CertificateTemplate>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CertificateTemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.TemplateFile, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.templateStatus, opt => opt.MapFrom(_ => TemplateStatus.Active));

            // Mapping for Create Certificate Template Response
            CreateMap<CertificateTemplate, CreateCertificateTemplateResponse>()
                .ForMember(dest => dest.TemplateStatus, opt => opt.MapFrom(src => src.templateStatus.ToString()));

            // Mapping for Get Certificate Template Response
            CreateMap<CertificateTemplate, GetCertificateTemplateResponse>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreateByUser != null ? src.CreateByUser.FullName : null))
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null))
                .ForMember(dest => dest.TemplateStatus, opt => opt.MapFrom(src => src.templateStatus.ToString()));

            // Mapping for Get All Certificate Templates Response
            CreateMap<CertificateTemplate, GetAllCertificateTemplatesResponse.CertificateTemplateItem>()
                .ForMember(dest => dest.TemplateStatus, opt => opt.MapFrom(src => src.templateStatus.ToString()));

            CreateMap<CreateCertificateTemplateDTO, CertificateTemplate>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.templateStatus, opt => opt.MapFrom(src => TemplateStatus.Inactive))
                .ForMember(dest => dest.CertificateTemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.TemplateFile, opt => opt.Ignore())
                .ForMember(dest => dest.TemplateName, opt => opt.Ignore());

            // Map from entity to response models
            CreateMap<CertificateTemplate, CreateCertificateTemplateResponse>();

            CreateMap<CertificateTemplate, GetCertificateTemplateResponse>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreateByUser.FullName))
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null));

            CreateMap<CertificateTemplate, GetAllCertificateTemplatesResponse.CertificateTemplateItem>();

            CreateMap<CertificateTemplate, UpdateCertificateTemplateResponse>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreateByUser.FullName))
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null))
                .ForMember(dest => dest.TemplateStatus, opt => opt.MapFrom(src => src.templateStatus));

            CreateMap<Certificate, CertificateModel>()
                .ForMember(dest => dest.TemplateId, opt => opt.MapFrom(src => src.CertificateTemplateId))
                .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate));
            CreateMap<CreateDecisionTemplateDTO, DecisionTemplate>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.templateName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.TemplateContent, opt => opt.Ignore()); // Được xử lý trong service

            // Response từ Entity sau Create
            CreateMap<DecisionTemplate, CreateDecisionTemplateResponse>()
                .ForMember(dest => dest.TemplateContentWithSas, opt => opt.Ignore()); // Được xử lý trong service

            // Response từ Entity cho Get by ID
            CreateMap<DecisionTemplate, DecisionTemplateModel>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null ? $"{src.CreatedByUser.FullName}" : string.Empty))
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src =>
                    src.ApprovedByUser != null ? $"{src.ApprovedByUser.FullName}" : string.Empty))
                .ForMember(dest => dest.TemplateContentWithSas, opt => opt.Ignore()); // Được xử lý trong service

            CreateMap<DecisionTemplate, GetAllDecisionTemplatesResponse.DecisionTemplateItem>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null ? $"{src.CreatedByUser.FullName}" : string.Empty))
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src =>
                    src.ApprovedByUser != null ? $"{src.ApprovedByUser.FullName}" : string.Empty));

            // Update DTO to Entity
            CreateMap<UpdateDecisionTemplateDTO, DecisionTemplate>()
                .ForMember(dest => dest.TemplateContent, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Response từ Entity sau Update
            CreateMap<DecisionTemplate, UpdateDecisionTemplateResponse>()
                .ForMember(dest => dest.TemplateContentWithSas, opt => opt.Ignore()); // Được xử lý trong service

            CreateMap<CreateDecisionDTO, Decision>()
                .ForMember(dest => dest.DecisionId, opt => opt.Ignore())
                .ForMember(dest => dest.DecisionCode, opt => opt.Ignore())
                .ForMember(dest => dest.Title, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.DecisionStatus, opt => opt.MapFrom(src => DecisionStatus.Draft))
                .ForMember(dest => dest.CertificateId, opt => opt.Ignore())
                .ForMember(dest => dest.DecisionTemplateId, opt => opt.Ignore());

            CreateMap<Decision, CreateDecisionResponse>()
                .ForMember(dest => dest.DecisionId, opt => opt.MapFrom(src => src.DecisionId))
                .ForMember(dest => dest.DecisionCode, opt => opt.MapFrom(src => src.DecisionCode))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate))
                .ForMember(dest => dest.IssuedByUserId, opt => opt.MapFrom(src => src.IssuedByUserId))
                .ForMember(dest => dest.DecisionTemplateId, opt => opt.MapFrom(src => src.DecisionTemplateId))
                .ForMember(dest => dest.DecisionStatus, opt => opt.MapFrom(src => src.DecisionStatus));


            CreateMap<Decision, DecisionModel>()
                .ForMember(dest => dest.DecisionCode, opt => opt.MapFrom(src => src.DecisionCode))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.ContentWithSas, opt => opt.Ignore())
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate))
                .ForMember(dest => dest.IssuedBy, opt => opt.MapFrom(src => src.IssuedByUserId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.DecisionStatus))
                .ForMember(dest => dest.DecisionTemplateId, opt => opt.MapFrom(src => src.DecisionTemplateId));
        }

    }

}

