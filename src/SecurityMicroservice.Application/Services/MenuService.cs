using AutoMapper;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Application.Services;

public interface IMenuService
{
    Task<MenuDto?> GetMenuByApplicationAsync(string applicationCode, Guid userId);
    Task<List<MenuDto>> GetAllUserMenusAsync(Guid userId);
}

public class MenuService : IMenuService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public MenuService(
        IUserRepository userRepository, 
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<MenuDto?> GetMenuByApplicationAsync(string applicationCode, Guid userId)
    {
        // Obtener los permisos del usuario para la aplicación específica
        var userPermissions = await _userRepository.GetUserPermissionEntitiesAsync(userId);
        
        // Filtrar permisos por aplicación
        var appPermissions = userPermissions
            .Where(p => p.Option.Module.Application.Code == applicationCode)
            .ToList();

        if (!appPermissions.Any())
        {
            return null; // Usuario no tiene permisos en esta aplicación
        }

        // Obtener la aplicación
        var application = appPermissions.First().Option.Module.Application;

        // Agrupar por módulos
        var moduleGroups = appPermissions
            .GroupBy(p => p.Option.Module)
            .OrderBy(g => g.Key.Order)
            .ToList();

        var menuDto = new MenuDto
        {
            ApplicationIcon = application.Icon,
            ApplicationUrl = application.BaseUrl,
            ApplicationCode = application.Code,
            ApplicationName = application.Name,
            Modules = new List<ModuleDto>()
        };

        foreach (var moduleGroup in moduleGroups)
        {
            var module = moduleGroup.Key;
            
            // Agrupar por opciones
            var optionGroups = moduleGroup
                .GroupBy(p => p.Option)
                .ToList();

            var moduleDto = new ModuleDto
            {
                ModuleId = module.ModuleId,
                Code = module.Code,
                Name = module.Name,
                Description = module.Description,
                Icon = module.Icon,
                Order = module.Order,
                Options = new List<MenuOptionDto>()
            };

            foreach (var optionGroup in optionGroups)
            {
                var option = optionGroup.Key;
                var allowedActions = optionGroup
                    .Where(p => p.IsActive)
                    .Select(p => p.ActionCode)
                    .Distinct()
                    .ToList();

                if (allowedActions.Any())
                {
                    var optionDto = new MenuOptionDto
                    {
                        OptionId = option.OptionId,
                        Code = option.Code,
                        Name = option.Name,
                        Route = option.Route,
                        HttpMethod = option.HttpMethod,
                        Icon = option.Icon,
                        AllowedActions = allowedActions
                    };

                    moduleDto.Options.Add(optionDto);
                }
            }

            if (moduleDto.Options.Any())
            {
                menuDto.Modules.Add(moduleDto);
            }
        }

        return menuDto.Modules.Any() ? menuDto : null;
    }

    public async Task<List<MenuDto>> GetAllUserMenusAsync(Guid userId)
    {
        // Obtener todos los permisos del usuario
        var userPermissions = await _userRepository.GetUserPermissionEntitiesAsync(userId);
        
        if (!userPermissions.Any())
        {
            return new List<MenuDto>();
        }

        // Agrupar por aplicación
        var applicationGroups = userPermissions
            .GroupBy(p => p.Option.Module.Application)
            .ToList();

        var menus = new List<MenuDto>();

        foreach (var appGroup in applicationGroups)
        {
            var application = appGroup.Key;
            
            // Agrupar por módulos dentro de esta aplicación
            var moduleGroups = appGroup
                .GroupBy(p => p.Option.Module)
                .OrderBy(g => g.Key.Order)
                .ToList();

            var menuDto = new MenuDto
            {
                ApplicationCode = application.Code,
                ApplicationName = application.Name,
                Modules = new List<ModuleDto>()
            };

            foreach (var moduleGroup in moduleGroups)
            {
                var module = moduleGroup.Key;
                
                // Agrupar por opciones
                var optionGroups = moduleGroup
                    .GroupBy(p => p.Option)
                    .ToList();

                var moduleDto = new ModuleDto
                {
                    ModuleId = module.ModuleId,
                    Code = module.Code,
                    Name = module.Name,
                    Description = module.Description,
                    Icon = module.Icon,
                    Order = module.Order,
                    Options = new List<MenuOptionDto>()
                };

                foreach (var optionGroup in optionGroups)
                {
                    var option = optionGroup.Key;
                    var allowedActions = optionGroup
                        .Where(p => p.IsActive)
                        .Select(p => p.ActionCode)
                        .Distinct()
                        .ToList();

                    if (allowedActions.Any())
                    {
                        var optionDto = new MenuOptionDto
                        {
                            OptionId = option.OptionId,
                            Code = option.Code,
                            Name = option.Name,
                            Route = option.Route,
                            HttpMethod = option.HttpMethod,
                            Icon = option.Icon,
                            AllowedActions = allowedActions
                        };

                        moduleDto.Options.Add(optionDto);
                    }
                }

                if (moduleDto.Options.Any())
                {
                    menuDto.Modules.Add(moduleDto);
                }
            }

            if (menuDto.Modules.Any())
            {
                menus.Add(menuDto);
            }
        }

        return menus;
    }
}