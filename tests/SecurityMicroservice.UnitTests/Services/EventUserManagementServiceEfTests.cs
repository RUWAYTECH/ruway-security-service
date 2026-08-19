using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Ruway.Events.Command.Interfaces.Events;
using Ruway.Security.Subscription.Hub.Services;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Application.Mappings;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.DTOs;
using Xunit;
using AppEntity = SecurityMicroservice.Domain.Entities.Application;

namespace SecurityMicroservice.UnitTests.Services;

/// <summary>
/// Pruebas contra el stack real de EF (no mocks de servicios). Son las únicas que detectan los
/// conflictos de tracking: los repositorios consultan con AsNoTracking y hacen Attach al
/// actualizar, así que dos operaciones sobre la misma fila compartiendo DbContext revientan.
/// </summary>
public class EventUserManagementServiceEfTests : IDisposable
{
    private const string PortalCliente = "PORTALCLIENTE";
    private const string MiTalla = "MITALLA";
    private const string RolBeneficiario = "PCB01";

    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly ServiceProvider _provider;
    private readonly EventUserManagementService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _portalClienteId = Guid.NewGuid();
    private readonly Guid _miTallaId = Guid.NewGuid();
    private readonly Guid _rolBeneficiarioId = Guid.NewGuid();
    private readonly Guid _rolMiTallaId = Guid.NewGuid();

    public EventUserManagementServiceEfTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<SecurityDbContext>(o => o.UseInMemoryDatabase(_dbName), ServiceLifetime.Scoped);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserApplicationRepository, UserApplicationRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();

        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IUserApplicationService, UserApplicationService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();

        var config = new MapperConfiguration(c =>
        {
            c.AddProfile<MappingProfile>();
            c.AddProfile<MappingDtoToEntity>();
        });
        services.AddSingleton<IMapper>(config.CreateMapper());

        var publisher = new Mock<IEventPublisher>();
        publisher.Setup(p => p.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(publisher.Object);

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton(new Mock<IEmailService>().Object);
        services.AddSingleton(Options.Create(new WebAppSettings { Url = "http://portal", PortalInternoUrl = "http://interno" }));
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        Seed();

        _service = new EventUserManagementService(_provider, NullLogger<EventUserManagementService>.Instance);
    }

    private void Seed()
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();

        db.Applications.AddRange(
            new AppEntity { ApplicationId = _portalClienteId, Code = PortalCliente, Name = "Portal Cliente", BaseUrl = "http://pc", IsActive = true },
            new AppEntity { ApplicationId = _miTallaId, Code = MiTalla, Name = "Mi Talla", BaseUrl = "http://mt", IsActive = true });

        db.Roles.AddRange(
            new Role { RoleId = _rolBeneficiarioId, ApplicationId = _portalClienteId, Code = RolBeneficiario, Name = "Beneficiario", IsActive = true },
            new Role { RoleId = _rolMiTallaId, ApplicationId = _miTallaId, Code = "MT01", Name = "Colaborador", IsActive = true });

        // Beneficiario tal como lo crea el alta: sin EmployeeId, con acceso y rol en PORTALCLIENTE.
        db.Users.Add(new User
        {
            UserId = _userId,
            UserName = "90000006",
            PasswordHash = "hash",
            FirstName = "Ana",
            LastName = "Quispe",
            Email = "ana@test.com",
            Status = UserStatus.Active,
            EmployeeId = null
        });
        db.UserApplications.Add(new UserApplication
        {
            UserId = _userId,
            ApplicationId = _portalClienteId,
            IsActive = true,
            AssignedAt = DateTime.UtcNow
        });
        db.UserRoles.Add(new UserRole { UserId = _userId, RoleId = _rolBeneficiarioId });

        db.SaveChanges();
    }

    private PeopleUpdatedEvent EdicionOrdinaria(string apellido = "Quispe Rojas") =>
        new(_userId, null, "Ana", apellido, "90000006", "ana@test.com", "ana@test.com",
            "999888777", PortalCliente, RolBeneficiario, true, IsActive: true);

    private (bool AppActiva, int Roles) LeerAcceso()
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();

        var ua = db.UserApplications
            .AsNoTracking()
            .SingleOrDefault(x => x.UserId == _userId && x.ApplicationId == _portalClienteId);

        var roles = db.UserRoles
            .AsNoTracking()
            .Count(ur => ur.UserId == _userId && ur.RoleId == _rolBeneficiarioId);

        return (ua?.IsActive ?? false, roles);
    }

    [Fact]
    public async Task EditarUnBeneficiario_NoLeQuitaElAcceso()
    {
        var antes = LeerAcceso();
        antes.AppActiva.Should().BeTrue();
        antes.Roles.Should().Be(1);

        var result = await _service.UpdateUserFromPeopleAsync(EdicionOrdinaria());

        result.Success.Should().BeTrue(because: "una edición ordinaria no puede fallar, pero falló con: {0}", result.Message);

        var despues = LeerAcceso();
        despues.AppActiva.Should().BeTrue(because: "editar un beneficiario no debe revocarle el acceso");
        despues.Roles.Should().Be(1, because: "debe conservar su rol PCB01");
    }

    [Fact]
    public async Task EditarUnBeneficiario_PropagaLosDatosEditados()
    {
        await _service.UpdateUserFromPeopleAsync(EdicionOrdinaria(apellido: "Quispe Rojas"));

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        db.Users.AsNoTracking().Single(u => u.UserId == _userId).LastName.Should().Be("Quispe Rojas");
    }

    [Fact]
    public async Task EditarDosVecesSeguidas_SigueConservandoElAcceso()
    {
        await _service.UpdateUserFromPeopleAsync(EdicionOrdinaria("Primera"));
        await _service.UpdateUserFromPeopleAsync(EdicionOrdinaria("Segunda"));

        var despues = LeerAcceso();
        despues.AppActiva.Should().BeTrue();
        despues.Roles.Should().Be(1);
    }

    [Fact]
    public async Task DesactivarBeneficiarioConDosAplicaciones_SoloRevocaLaSuya()
    {
        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
            db.UserApplications.Add(new UserApplication
            {
                UserId = _userId,
                ApplicationId = _miTallaId,
                IsActive = true,
                AssignedAt = DateTime.UtcNow
            });
            db.UserRoles.Add(new UserRole { UserId = _userId, RoleId = _rolMiTallaId });
            db.SaveChanges();
        }

        var baja = new PeopleUpdatedEvent(_userId, null, "Ana", "Quispe", "90000006", "ana@test.com",
            "ana@test.com", "999888777", PortalCliente, RolBeneficiario, true, IsActive: false);

        var result = await _service.UpdateUserFromPeopleAsync(baja);

        result.Success.Should().BeTrue();

        using var check = _provider.CreateScope();
        var ctx = check.ServiceProvider.GetRequiredService<SecurityDbContext>();

        ctx.UserApplications.AsNoTracking()
            .Single(x => x.UserId == _userId && x.ApplicationId == _portalClienteId)
            .IsActive.Should().BeFalse();

        ctx.UserApplications.AsNoTracking()
            .Single(x => x.UserId == _userId && x.ApplicationId == _miTallaId)
            .IsActive.Should().BeTrue(because: "MITALLA debe quedar intacto");

        ctx.Users.AsNoTracking().Single(u => u.UserId == _userId)
            .Status.Should().Be(UserStatus.Active, because: "le queda MITALLA");
    }

    public void Dispose() => _provider.Dispose();
}
