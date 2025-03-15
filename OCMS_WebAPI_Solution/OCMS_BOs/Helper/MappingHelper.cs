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
        }
    }
}
