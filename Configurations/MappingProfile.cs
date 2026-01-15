using AutoMapper;
using LogLog.Service.Domain.Entities;

namespace LogLog.Service.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Avatar, AvatarDto>();
        }
    }
}
