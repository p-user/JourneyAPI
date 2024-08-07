using AutoMapper;
using DataAccessLayer.Entities;
using SharedLayer.Dtos;


namespace BusinessLayer.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<Journey, JourneyDto>()
                .ReverseMap();
            CreateMap<Journey, JourneyFilterDto>()
               .ReverseMap();
            CreateMap<Journey, ViewJourneyDto>()
              .ReverseMap();
            CreateMap<JourneyShare, SharedJourneyDto>()
              .ReverseMap();
            CreateMap<ApplicationRole, ApplicationRoleDto>()
              .ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserDto>()
              .ReverseMap();
        }
    }
}
