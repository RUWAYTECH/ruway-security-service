using AutoMapper;

namespace SecurityMicroservice.Application.Mappings
{
    public class MappingDtoToEntity : Profile
    {
        public MappingDtoToEntity()
        {
            CreateMap<Shared.DTOs.CreateRoleRequest, Domain.Entities.Role>();
            CreateMap<Shared.DTOs.UpdateRoleRequest, Domain.Entities.Role>();
        }
    }
}
