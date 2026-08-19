using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ruway.Events.Command.Interfaces.Events;
using Ruway.Security.Subscription.Hub.Services;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;
using SecurityMicroservice.Shared.Request.UserApplication;
using SecurityMicroservice.Shared.Response.Common;
using SecurityMicroservice.Shared.Response.User;
using Xunit;

namespace SecurityMicroservice.UnitTests.Services;

/// <summary>
/// Cubre el manejo de eventos de personas: revocación acotada a una aplicación y no dejar al
/// usuario sin accesos cuando algo del evento no se puede resolver.
/// </summary>
public class EventUserManagementServiceTests
{
    private const string PortalCliente = "PORTALCLIENTE";
    private const string MiTalla = "MITALLA";
    private const string RolBeneficiario = "PCB01";

    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PortalClienteId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid MiTallaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid RolBeneficiarioId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IUserApplicationService> _userAppService = new();
    private readonly Mock<IApplicationService> _appService = new();
    private readonly Mock<IRoleService> _roleService = new();

    public EventUserManagementServiceTests()
    {
        _userService
            .Setup(s => s.GetById(It.IsAny<object>()))
            .ReturnsAsync(ResponseDto.Create(new UserResponseDto { UserId = UserId, UserName = "70123456" }));
        _userService
            .Setup(s => s.Delete(It.IsAny<object>()))
            .ReturnsAsync(ResponseDto.Create());
        _userService
            .Setup(s => s.UpdatePartial(It.IsAny<Guid>(), It.IsAny<BaseUserRequestDto>(), It.IsAny<string?>()))
            .ReturnsAsync(ResponseDto.Create<BaseUserRequestDto>(new UserRequestDto { UserId = UserId, Username = "70123456" }));
        _userService
            .Setup(s => s.PhysicallyDelete(It.IsAny<Guid>()))
            .ReturnsAsync(ResponseDto.Create());

        SetupApplication(PortalCliente, PortalClienteId);
        SetupApplication(MiTalla, MiTallaId);

        _roleService
            .Setup(s => s.GetByApplicationIdAsync(PortalClienteId))
            .ReturnsAsync(new List<RoleDto>
            {
                new() { RoleId = RolBeneficiarioId, ApplicationId = PortalClienteId, Code = RolBeneficiario, IsActive = true }
            });

        _userAppService
            .Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(ResponseDto.Create());
        _userAppService
            .Setup(s => s.CreateAsync(It.IsAny<CreateUserApplicationRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(ResponseDto.Create(new UserApplicationDto()));

        // Por defecto el usuario tiene acceso a ambas aplicaciones.
        SetupAssignments(
            new UserApplicationDto { UserId = UserId, ApplicationId = PortalClienteId, IsActive = true },
            new UserApplicationDto { UserId = UserId, ApplicationId = MiTallaId, IsActive = true });
    }

    private void SetupApplication(string code, Guid applicationId) =>
        _appService
            .Setup(s => s.GetByCodeAsync(code))
            .ReturnsAsync(ResponseDto.Create(new ApplicationDto { ApplicationId = applicationId, Code = code, IsActive = true }));

    private void SetupAssignments(params UserApplicationDto[] assignments) =>
        _userAppService
            .Setup(s => s.GetByUserIdAsync(UserId))
            .ReturnsAsync(assignments.ToList());

    private EventUserManagementService BuildService()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => _userService.Object);
        services.AddScoped(_ => _userAppService.Object);
        services.AddScoped(_ => _appService.Object);
        services.AddScoped(_ => _roleService.Object);

        return new EventUserManagementService(
            services.BuildServiceProvider(),
            NullLogger<EventUserManagementService>.Instance);
    }

    private static PeopleUpdatedEvent PeopleUpdated(string applicationCode, string roleCode, bool isActive) =>
        new(UserId, null, "Ana", "Quispe", "70123456", "ana@test.com", "ana@test.com",
            "999888777", applicationCode, roleCode, true, isActive);

    [Fact]
    public async Task IsActiveFalse_RevocaSoloEsaAplicacion()
    {
        var result = await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, RolBeneficiario, isActive: false));

        result.Success.Should().BeTrue();
        _userAppService.Verify(s => s.DeleteAsync(UserId, PortalClienteId, false), Times.Once);
        _userAppService.Verify(s => s.DeleteAsync(UserId, MiTallaId, It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task IsActiveFalse_NoVuelveAOtorgarElRolDelEvento()
    {
        await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, RolBeneficiario, isActive: false));

        _userAppService.Verify(s => s.CreateAsync(It.IsAny<CreateUserApplicationRequest>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task IsActiveFalse_SiLeQuedanOtrasApps_NoDesactivaAlUsuario()
    {
        await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, RolBeneficiario, isActive: false));

        _userService.Verify(s => s.Delete(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task IsActiveFalse_SiEraSuUltimaApp_DesactivaAlUsuario()
    {
        SetupAssignments(new UserApplicationDto { UserId = UserId, ApplicationId = PortalClienteId, IsActive = false });

        var result = await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, RolBeneficiario, isActive: false));

        result.Success.Should().BeTrue();
        _userService.Verify(s => s.Delete(It.Is<object>(id => (Guid)id == UserId)), Times.Once);
    }

    [Fact]
    public async Task IsActiveFalse_ConAplicacionInexistente_NoRevocaNada()
    {
        _appService
            .Setup(s => s.GetByCodeAsync("NO_EXISTE"))
            .ReturnsAsync(ResponseDto.Create<ApplicationDto>(default(ApplicationDto)));

        var result = await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated("NO_EXISTE", RolBeneficiario, isActive: false));

        result.Success.Should().BeFalse();
        _userAppService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        _userService.Verify(s => s.Delete(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Update_ConRolInexistente_NoRevocaElAccesoQueYaTenia()
    {
        var result = await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, "NO_EXISTE", isActive: true));

        result.Success.Should().BeFalse(because: "no se puede resolver el rol del evento");
        _userAppService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        _userAppService.Verify(s => s.CreateAsync(It.IsAny<CreateUserApplicationRequest>(), It.IsAny<bool>()), Times.Never);
        _userService.Verify(s => s.UpdatePartial(It.IsAny<Guid>(), It.IsAny<BaseUserRequestDto>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Update_ConRolEnDistintoCase_LoResuelveIgual()
    {
        var result = await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, " pcb01 ", isActive: true));

        result.Success.Should().BeTrue();
        _userAppService.Verify(
            s => s.CreateAsync(It.Is<CreateUserApplicationRequest>(r => r.RoleIds!.Contains(RolBeneficiarioId)), false),
            Times.Once);
    }

    [Fact]
    public async Task Update_CuandoFallaLaAsignacionDelRol_DevuelveError()
    {
        _userAppService
            .Setup(s => s.CreateAsync(It.IsAny<CreateUserApplicationRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(ResponseDto.Error<UserApplicationDto>("no se pudo asignar"));

        var result = await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, RolBeneficiario, isActive: true));

        result.Success.Should().BeFalse(because: "dejar al usuario sin rol no puede reportarse como éxito");
        result.Message.Should().Contain("no se pudo asignar");
    }

    [Fact]
    public async Task Update_SinAsignacionActiva_NoIntentaRevocarAntesDeAsignar()
    {
        SetupAssignments(new UserApplicationDto { UserId = UserId, ApplicationId = MiTallaId, IsActive = true });

        var result = await BuildService().UpdateUserFromPeopleAsync(PeopleUpdated(PortalCliente, RolBeneficiario, isActive: true));

        result.Success.Should().BeTrue(because: "una primera asignación no debe bloquearse por no haber nada que revocar");
        _userAppService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        _userAppService.Verify(s => s.CreateAsync(It.IsAny<CreateUserApplicationRequest>(), false), Times.Once);
    }
}
