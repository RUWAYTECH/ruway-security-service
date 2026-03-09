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
using MediatR;
using Microsoft.AspNetCore.Http;
using SecurityMicroservice.Shared.DTOs;

namespace Ruway.Security.Subscription.Hub.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar configuración de RabbitMQ
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        
        // Configurar Entity Framework
        services.AddDbContext<SecurityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        // Registrar servicios de eventos
        services.AddScoped<IEventSubscriber, EventSubscriber>();
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRabbitMQService, RabbitMQService>();
        services.AddScoped<IRoleService, RoleService>();
        
        // Registrar servicios de negocio
        services.AddScoped<EventUserManagementService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IUserApplicationService, UserApplicationService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();  
              
        // Registrar repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserApplicationRepository, UserApplicationRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Email service
        services.AddScoped<IEmailService, EmailService>();
        
        // Registrar AutoMapper
        services.AddAutoMapper(typeof(SecurityMicroservice.Application.Mappings.MappingProfile));
        
        return services;
    }
}