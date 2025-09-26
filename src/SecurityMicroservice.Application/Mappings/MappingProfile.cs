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

        // UserApplication mappings
        CreateMap<UserApplication, UserApplicationDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.ApplicationName, opt => opt.MapFrom(src => src.Application.Name))
            .ForMember(dest => dest.ApplicationCode, opt => opt.MapFrom(src => src.Application.Code));

        // UserRole mappings
        CreateMap<UserRole, UserRoleDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
            .ForMember(dest => dest.RoleCode, opt => opt.MapFrom(src => src.Role.Code))
            .ForMember(dest => dest.ApplicationName, opt => opt.MapFrom(src => src.Role.Application.Name))
            .ForMember(dest => dest.ApplicationCode, opt => opt.MapFrom(src => src.Role.Application.Code));

        // UserPermission mappings
        CreateMap<UserPermission, UserPermissionDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => $"{src.Permission.Option.Name}:{src.Permission.ActionCode}"))
            .ForMember(dest => dest.OptionName, opt => opt.MapFrom(src => src.Permission.Option.Name))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Permission.Option.Module.Name))
            .ForMember(dest => dest.ApplicationName, opt => opt.MapFrom(src => src.Permission.Option.Module.Application.Name))
            .ForMember(dest => dest.ApplicationCode, opt => opt.MapFrom(src => src.Permission.Option.Module.Application.Code))
            .ForMember(dest => dest.ActionCode, opt => opt.MapFrom(src => src.Permission.ActionCode))
            .ForMember(dest => dest.GrantedByUsername, opt => opt.MapFrom(src => src.GrantedByUser != null ? src.GrantedByUser.Username : string.Empty));
    }
}