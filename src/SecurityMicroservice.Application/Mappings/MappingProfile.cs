using AutoMapper;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Response.Permission;

namespace SecurityMicroservice.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles.Where(ur => ur.Role.IsActive).Select(ur => ur.Role)));

        CreateMap<Domain.Entities.Application, ApplicationDto>();
        
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.ApplicationCode, opt => opt.MapFrom(src => src.Application.Code));

        CreateMap<Permission, PermissionResponseDto>()
            .ForMember(dest => dest.OptionName, opt => opt.MapFrom(src => src.Option.Name))
            .ForMember(dest => dest.OptionRoute, opt => opt.MapFrom(src => src.Option.Route))
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.Option.HttpMethod))
            .ForMember(dest => dest.ModuleCode, opt => opt.MapFrom(src => src.Option.Module.Code))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Option.Module.Name));

        CreateMap<Option, OptionDto>()
            .ForMember(dest => dest.ApplicationCode, opt => opt.MapFrom(src => src.Module.Application.Code));
    }
}