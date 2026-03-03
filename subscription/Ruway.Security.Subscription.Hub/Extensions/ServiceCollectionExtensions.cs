using Microsoft.EntityFrameworkCore;
using Ruway.Events.Command.Configuration;
using Ruway.Events.Command.Events;
using Ruway.Events.Command.Interfaces.Events;
using Ruway.Security.Subscription.Hub.Services;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Infrastructure.Data;
using AutoMapper;

namespace Ruway.Security.Subscription.Hub.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar configuración de RabbitMQ
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));
        
        // Configurar Entity Framework
        services.AddDbContext<SecurityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        // Registrar servicios de eventos
        services.AddScoped<IEventSubscriber, EventSubscriber>();
        
        // Registrar servicios de negocio
        services.AddScoped<EventUserManagementService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        
        // Registrar repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        
        // Registrar AutoMapper
        services.AddAutoMapper(typeof(SecurityMicroservice.Application.Mappings.MappingProfile));
        
        return services;
    }
}