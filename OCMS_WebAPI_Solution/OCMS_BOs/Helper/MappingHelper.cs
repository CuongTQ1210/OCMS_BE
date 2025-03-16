using AutoMapper;
using OCMS_BOs.Entities;
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
        }
    }
}
