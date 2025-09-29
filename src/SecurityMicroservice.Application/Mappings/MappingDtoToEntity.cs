using AutoMapper;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Shared.Request.Permission;

namespace SecurityMicroservice.Application.Mappings
{
    public class MappingDtoToEntity : Profile
    {
        public MappingDtoToEntity()
        {
            CreateMap<Shared.DTOs.CreateRoleRequest, Domain.Entities.Role>();
            CreateMap<Shared.DTOs.UpdateRoleRequest, Domain.Entities.Role>();
            CreateMap<PermissionRequestDto, Permission>();
        }
    }
}
