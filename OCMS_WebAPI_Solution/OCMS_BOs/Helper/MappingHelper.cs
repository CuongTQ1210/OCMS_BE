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
            CreateMap<Subject, GetAllSubjectModel>();
            CreateMap<User, UserModel>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl)) // ✅ Added mapping
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.SpecialtyId, opt => opt.MapFrom(src => src.SpecialtyId))
                .ForMember(dest => dest.AccountStatus, opt=> opt.MapFrom(src=>src.Status.ToString()))
                .ReverseMap();
            CreateMap<UserUpdateDTO, User>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
            CreateMap<CreateUserDTO, User>();
            CreateMap<CourseSubjectSpecialty, CourseSubjectSpecialtySimpleModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseName : string.Empty))
                .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.SubjectId))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : string.Empty))
                .ForMember(dest => dest.SpecialtyId, opt => opt.MapFrom(src => src.SpecialtyId))
                .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty != null ? src.Specialty.SpecialtyName : string.Empty))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Notes));
            CreateMap<CandidateUpdateDTO, Candidate>()
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore()) // Bỏ qua CandidateId
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Bỏ qua CreatedAt
                .ForMember(dest => dest.ImportRequestId, opt => opt.Ignore()) // Bỏ qua ImportRequestId
                .ForMember(dest => dest.ImportByUserID, opt => opt.Ignore()) // Bỏ qua ImportByUserID
                .ForMember(dest => dest.CandidateStatus, opt => opt.Ignore()) // Bỏ qua CandidateStatus
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Bỏ qua UpdatedAt
CreateMap<Subject, SubjectSimpleModel>();
            CreateMap<ExternalCertificateCreateDTO, ExternalCertificate>()
            .ForMember(dest => dest.VerifyByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Candidate, opt => opt.Ignore())
            .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => VerificationStatus.Pending))
            .ForMember(dest => dest.VerifyDate, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.CertificateFileURL, opt => opt.Ignore()) // Assuming you upload this separately
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now));

            CreateMap<ExternalCertificateUpdateDTO, ExternalCertificate>()
    .ForMember(dest => dest.VerifyByUserId, opt => opt.Ignore())
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
                .ForMember(dest => dest.ActionByUserId, opt => opt.MapFrom(src => src.ApproveByUserId))
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(src => src.RequestType.ToString())) // Convert Enum to String
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // Convert Enum to String
            .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src => src.ApprovedDate));
            CreateMap<ViewModel.RequestModel, Request>()
                .ForMember(dest => dest.RequestUserId, opt => opt.MapFrom(src => src.RequestById))
                .ForMember(dest => dest.ApproveByUserId, opt => opt.MapFrom(src => src.ActionByUserId))
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
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description)); // Fix typo in property name           


            CreateMap<TrainingPlanDTO, TrainingPlan>();
            CreateMap<TrainingPlan, TrainingPlanDTO>();
            // Course Mapping
            CreateMap<Course, CourseModel>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CourseRelatedId, opt => opt.MapFrom(src => src.RelatedCourseId))
                .ForMember(dest => dest.CourseLevel, opt => opt.MapFrom(src => src.CourseLevel.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Progress.ToString()))
                .ForMember(dest => dest.ApproveByUserId, opt => opt.MapFrom(src => src.ApproveByUserId))
                .ForMember(dest => dest.ApprovalDate, opt => opt.MapFrom(src => src.ApprovalDate))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.Trainees, opt => opt.MapFrom(src => src.CourseSubjectSpecialties != null ?
                    src.CourseSubjectSpecialties.Where(css => css.Trainees != null)
                    .SelectMany(css => css.Trainees).ToList() : new List<TraineeAssign>()))
                .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.CourseSubjectSpecialties != null ?
                    src.CourseSubjectSpecialties.Where(css => css.Subject != null)
                    .Select(css => css.Subject).ToList() : new List<Subject>()))
                .ForMember(dest => dest.CourseSubjectSpecialties, opt => opt.Ignore());


            CreateMap<CourseDTO, Course>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RelatedCourseId, opt => opt.MapFrom(src => src.CourseRelatedId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ForMember(dest => dest.CourseLevel, opt => opt.MapFrom(src => Enum.Parse<CourseLevel>(src.CourseLevel)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApproveByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Progress, opt => opt.Ignore())
                .ForMember(dest => dest.CourseSubjectSpecialties, opt => opt.Ignore())
                .ForMember(dest => dest.TrainingPlans, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedCourses, opt => opt.Ignore());

            CreateMap<Course, CourseDTO>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CourseRelatedId, opt => opt.MapFrom(src => src.RelatedCourseId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ForMember(dest => dest.CourseLevel, opt => opt.MapFrom(src => src.CourseLevel.ToString()));

            CreateMap<CourseUpdateDTO, Course>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RelatedCourseId, opt => opt.MapFrom(src => src.CourseRelatedId))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Progress, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApproveByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApproveByUser, opt => opt.Ignore())
                .ForMember(dest => dest.TrainingPlans, opt => opt.Ignore()) // Replace with this line
                .ForMember(dest => dest.CourseSubjectSpecialties, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedCourses, opt => opt.Ignore());

            // CourseSubjectSpecialty Mappings
            CreateMap<CourseSubjectSpecialty, CourseSubjectSpecialtyModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.SubjectId))
                .ForMember(dest => dest.SpecialtyId, opt => opt.MapFrom(src => src.SpecialtyId))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialty))
                .ForMember(dest => dest.Trainees, opt => opt.MapFrom(src => src.Trainees != null ?
                    src.Trainees : new List<TraineeAssign>()))
                .ForMember(dest => dest.Instructors, opt => opt.MapFrom(src => src.Instructors != null ?
                    src.Instructors : new List<InstructorAssignment>()))
                .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules != null ?
                    src.Schedules : new List<TrainingSchedule>()));

            CreateMap<CourseSubjectSpecialtyDTO, CourseSubjectSpecialty>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.Trainees, opt => opt.Ignore())
                .ForMember(dest => dest.Instructors, opt => opt.Ignore())
                .ForMember(dest => dest.Schedules, opt => opt.Ignore());

            CreateMap<CourseSubjectSpecialty, CourseSubjectSpecialtyDTO>();
            // Subject Mappings
            CreateMap<Subject, SubjectModel>()
                .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.SubjectId))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.SubjectName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Credits, opt => opt.MapFrom(src => src.Credits))
                .ForMember(dest => dest.PassingScore, opt => opt.MapFrom(src => src.PassingScore))
                .ForMember(dest => dest.CreateByUserId, opt => opt.MapFrom(src => src.CreateByUserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.CourseSubjectSpecialties, opt => opt.MapFrom(src => src.CourseSubjectSpecialties != null ?
                    src.CourseSubjectSpecialties : new List<CourseSubjectSpecialty>()));

            CreateMap<SubjectDTO, Subject>()
                .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.SubjectId))
                .ForMember(dest => dest.CourseSubjectSpecialties, opt => opt.Ignore())
                .ForMember(dest => dest.CreateByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ReverseMap();           

            // Trainee Assignment Mappings
            CreateMap<TraineeAssign, TraineeAssignModel>()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => src.RequestStatus.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => Enum.Parse<RequestStatus>(src.RequestStatus)))
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(src => src.AssignDate == default ? DateTime.Now : src.AssignDate))
                .ForMember(dest => dest.ApprovalDate, opt => opt.MapFrom(src => src.ApprovalDate == default ? null : src.ApprovalDate))
                .ForMember(dest => dest.Trainee, opt => opt.Ignore())
                .ForMember(dest => dest.CourseSubjectSpecialty, opt => opt.Ignore())
                .ForMember(dest => dest.AssignByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ApproveByUser, opt => opt.Ignore())
                .ForMember(dest => dest.Request, opt => opt.Ignore());

            CreateMap<TraineeAssignDTO, TraineeAssign>()
                .ForMember(dest => dest.TraineeAssignId, opt => opt.Ignore())
                .ForMember(dest => dest.CourseSubjectSpecialtyId, opt => opt.Ignore()) // Map CourseId to CourseSubjectSpecialtyId in service
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Pending))
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.ApprovalDate, opt => opt.Ignore())
                .ForMember(dest => dest.ApproveByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.RequestId, opt => opt.Ignore())
                .ForMember(dest => dest.Request, opt => opt.Ignore())
                .ForMember(dest => dest.Trainee, opt => opt.Ignore())
                .ForMember(dest => dest.CourseSubjectSpecialty, opt => opt.Ignore())
                .ForMember(dest => dest.AssignByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ApproveByUser, opt => opt.Ignore());

            CreateMap<TraineeAssign, TraineeAssignDTO>()
                .ForMember(dest => dest.CourseSubjectSpecialtyId, opt => opt.Ignore()); // CourseId not directly mapped

            // Instructor Assignment Mappings
            CreateMap<InstructorAssignment, InstructorAssignmentModel>()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => src.RequestStatus.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => Enum.Parse<RequestStatus>(src.RequestStatus)))
                .ForMember(dest => dest.CourseSubjectSpecialty, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.AssignByUser, opt => opt.Ignore());

            CreateMap<InstructorAssignmentDTO, InstructorAssignment>()
                .ForMember(dest => dest.AssignmentId, opt => opt.Ignore())
                .ForMember(dest => dest.CourseSubjectSpecialtyId, opt => opt.Ignore()) // Map SubjectId to CourseSubjectSpecialtyId in service
                .ForMember(dest => dest.AssignByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Pending))
                .ForMember(dest => dest.CourseSubjectSpecialty, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.AssignByUser, opt => opt.Ignore());

            CreateMap<InstructorAssignment, InstructorAssignmentDTO>()
                .ForMember(dest => dest.CourseSubjectSpecialtyId, opt => opt.Ignore()); // SubjectId not directly mapped

            // Training Schedule Mappings
            CreateMap<TrainingSchedule, TrainingScheduleModel>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.CourseSubjectSpecialty.Subject.SubjectName))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null ? src.Instructor.FullName : null))
                .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.StartDateTime))
                .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.EndDateTime))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src => src.DaysOfWeek != null ? string.Join(", ", src.DaysOfWeek.Select(d => d.ToString())) : ""))
                .ForMember(dest => dest.SubjectPeriod, opt => opt.MapFrom(src => src.SubjectPeriod))
                .ReverseMap()
                .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src => src.DaysOfWeek.Split(", ", StringSplitOptions.RemoveEmptyEntries).Select(d => Enum.Parse<DayOfWeek>(d)).ToList()))
                .ForMember(dest => dest.CourseSubjectSpecialty, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<TrainingScheduleDTO, TrainingSchedule>()
                .ForMember(dest => dest.ScheduleID, opt => opt.Ignore())
                .ForMember(dest => dest.CourseSubjectSpecialtyId, opt => opt.Ignore()) // Map SubjectID to CourseSubjectSpecialtyId in service
                .ForMember(dest => dest.InstructorID, opt => opt.MapFrom(src => src.InstructorID))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.StartDay))
                .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.EndDay))
                .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src => src.DaysOfWeek != null ? src.DaysOfWeek.Select(d => (DayOfWeek)d).ToList() : new List<DayOfWeek>()))
                .ForMember(dest => dest.ClassTime, opt => opt.MapFrom(src => src.ClassTime))
                .ForMember(dest => dest.SubjectPeriod, opt => opt.MapFrom(src => src.SubjectPeriod))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ScheduleStatus.Pending))
                .ForMember(dest => dest.CourseSubjectSpecialty, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<TrainingSchedule, TrainingScheduleDTO>()
                .ForMember(dest => dest.CourseSubjectSpecialtyId, opt => opt.Ignore()) // SubjectID not directly mapped
                .ForMember(dest => dest.InstructorID, opt => opt.MapFrom(src => src.InstructorID))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.StartDateTime))
                .ForMember(dest => dest.EndDay, opt => opt.MapFrom(src => src.EndDateTime))
                .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src => src.DaysOfWeek != null ? src.DaysOfWeek.Select(d => (int)d).ToList() : new List<int>()))
                .ForMember(dest => dest.ClassTime, opt => opt.MapFrom(src => src.ClassTime))
                .ForMember(dest => dest.SubjectPeriod, opt => opt.MapFrom(src => src.SubjectPeriod));

            // Grade Mappings
            // Grade Mappings
            CreateMap<Grade, GradeModel>()
                .ForMember(dest => dest.GradeStatus, opt => opt.MapFrom(src => src.gradeStatus.ToString()))
                .ForMember(dest => dest.TraineeAssignId, opt => opt.MapFrom(src => src.TraineeAssignID))
                .ForMember(dest => dest.TraineeId, opt => opt.MapFrom(src => src.TraineeAssign.TraineeId))
                .ForMember(dest => dest.CourseSubjectSpecialtyId, opt => opt.MapFrom(src => src.TraineeAssign.CourseSubjectSpecialtyId))
                .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.TraineeAssign.Trainee.FullName))
                .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.TraineeAssign.CourseSubjectSpecialty.SubjectId))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.TraineeAssign.CourseSubjectSpecialty.Subject.SubjectName))
                .ReverseMap()
                .ForMember(dest => dest.gradeStatus, opt => opt.MapFrom(src => Enum.Parse<GradeStatus>(src.GradeStatus)));

            CreateMap<GradeDTO, Grade>()
                .ForMember(dest => dest.GradeId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalScore, opt => opt.Ignore())
                .ForMember(dest => dest.gradeStatus, opt => opt.Ignore())
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
                .ForMember(dest => dest.CertificateFileURL, opt => opt.Ignore()) // You'll set this after upload
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.VerifyByUserId, opt => opt.Ignore())
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

            CreateMap<Decision, CreateDecisionResponse>();

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