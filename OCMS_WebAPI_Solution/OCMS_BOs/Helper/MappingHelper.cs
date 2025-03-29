using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Helper
{
    public class MappingHelper : Profile
    {
        public MappingHelper() 
        {
            CreateMap<User, UserModel>();
            CreateMap<UserModel, User>();

            CreateMap<UserUpdateDTO, User>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<CandidateUpdateDTO, Candidate>()
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore()) // Bỏ qua CandidateId
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Bỏ qua CreatedAt
                .ForMember(dest => dest.ImportRequestId, opt => opt.Ignore()) // Bỏ qua ImportRequestId
                .ForMember(dest => dest.ImportByUserID, opt => opt.Ignore()) // Bỏ qua ImportByUserID
                .ForMember(dest => dest.CandidateStatus, opt => opt.Ignore()) // Bỏ qua CandidateStatus
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Bỏ qua UpdatedAt

            CreateMap<Specialties, SpecialtyModel>();
            CreateMap<SpecialtyModel, Specialties>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore());

            CreateMap<Request, ViewModel.RequestModel>()
                .ForMember(dest => dest.RequestById, opt => opt.MapFrom(src => src.RequestUserId))
                .ForMember(dest => dest.ApprovedById, opt => opt.MapFrom(src => src.ApprovedBy))
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(src => src.RequestType.ToString())) // Convert Enum to String
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())); // Convert Enum to String

            CreateMap<ViewModel.RequestModel, Request>()
                .ForMember(dest => dest.RequestUserId, opt => opt.MapFrom(src => src.RequestById))
                .ForMember(dest => dest.ApprovedBy, opt => opt.MapFrom(src => src.ApprovedById))
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(src => Enum.Parse<RequestType>(src.RequestType))) // Convert String to Enum
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<RequestStatus>(src.Status))); // Convert String to Enum
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
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false));

            CreateMap<TrainingPlan, TrainingPlanModel>()
                .ForMember(dest => dest.TrainingPlanStatus, opt => opt.MapFrom(src => src.TrainingPlanStatus.ToString()))
                .ForMember(dest => dest.CreateByUserName, opt => opt.MapFrom(src => src.CreateByUser.FullName))
                .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty.SpecialtyName))
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.Courses))
                .ReverseMap();
            CreateMap<TrainingPlanDTO, TrainingPlan>();
            CreateMap<TrainingPlan, TrainingPlanDTO>();
            // Course Mapping
            CreateMap<Course, CourseModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Trainees, opt => opt.MapFrom(src => src.Trainees))
                .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects))
                .ReverseMap();
            CreateMap<CourseDTO, Course>();
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
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(src => src.AssignDate == default ? DateTime.UtcNow : src.AssignDate)) // Default AssignDate
                .ForMember(dest => dest.ApprovalDate, opt => opt.MapFrom(src => src.ApprovalDate == default ? null : src.ApprovalDate)); // Keep null if not approved

            // Mapping TraineeAssignDTO → TraineeAssign (Used for Creating Assignments)
            CreateMap<TraineeAssignDTO, TraineeAssign>()
                .ForMember(dest => dest.TraineeAssignId, opt => opt.Ignore()) // Ignore ID, auto-generated
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Pending)) // Default to Pending
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(_ => DateTime.UtcNow)) // Set AssignDate to now
                .ForMember(dest => dest.ApprovalDate, opt => opt.Ignore()) // ApprovalDate ignored during creation
                .ForMember(dest => dest.ApproveByUserId, opt => opt.Ignore()) // Approval user not set at creation
                .ForMember(dest => dest.RequestId, opt => opt.Ignore()) // Request will be assigned later
                .ForMember(dest => dest.Request, opt => opt.Ignore()); // Ignore navigation property
            CreateMap<TraineeAssign, TraineeAssignDTO>();
            // Instructor Assignment Mapping
            CreateMap<InstructorAssignment, InstructorAssignmentModel>()
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => src.RequestStatus.ToString()))
                .ReverseMap();
            CreateMap<TrainingSchedule, TrainingScheduleModel>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.SubjectName))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.FullName))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.FullName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();

            CreateMap<CourseParticipant, CourseParticipantModel>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();

            CreateMap<CourseParticipantDto, CourseParticipant>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<CourseParticipantStatus>(src.Status)))
                .ReverseMap();
            CreateMap<ExternalCertificateModel, ExternalCertificate>()
                .ForMember(dest => dest.CertificateCode, opt => opt.MapFrom(src => src.CertificateCode))
                .ForMember(dest => dest.CertificateName, opt => opt.MapFrom(src => src.CertificateName))
                .ForMember(dest => dest.IssuingOrganization, opt => opt.MapFrom(src => src.CertificateProvider))
                .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.CandidateId))
                .ForMember(dest => dest.CertificateFileURL, opt => opt.Ignore());

            CreateMap<ExternalCertificate, ExternalCertificateModel>();
        }


    }
}
