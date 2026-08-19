using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Ruway.Events.Command.Interfaces.Events;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;
using SecurityMicroservice.Shared.Response.User;
using Xunit;

namespace SecurityMicroservice.UnitTests.Services;

/// <summary>
/// El alta de usuario tiene que sobrevivir a un fallo de correo: si no, el llamador la da por
/// fallida y nunca le asigna aplicación ni rol, dejando al usuario creado pero inservible.
/// </summary>
public class UserServiceCreateTests : IDisposable
{
    private readonly SecurityDbContext _context;
    private readonly UserService _userService;
    private readonly Mock<IEmailService> _emailService = new();

    public UserServiceCreateTests()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SecurityDbContext(options);

        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<UserResponseDto>(It.IsAny<object>()))
            .Returns((object src) =>
            {
                var u = (User)src;
                return new UserResponseDto { UserId = u.UserId, UserName = u.UserName, EmployeeId = u.EmployeeId };
            });

        var publisher = new Mock<IEventPublisher>();
        publisher.Setup(p => p.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userService = new UserService(
            new UserRepository(_context),
            new PasswordService(),
            mapper.Object,
            _emailService.Object,
            Options.Create(new WebAppSettings { Url = "http://portal", PortalInternoUrl = "http://interno" }),
            NullLogger<UserService>.Instance,
            publisher.Object);
    }

    private const string PortalBeneficiario = "http://interno";
    private const string PanelAdmin = "http://portal";

    private static UserRequestDto NewRequest(Guid? employeeId = null, bool isExternal = false) => new()
    {
        UserId = Guid.NewGuid(),
        Username = "70123456",
        Password = "Temporal123",
        FirstName = "Ana",
        LastName = "Quispe",
        Email = "ana@test.com",
        EmployeeId = employeeId,
        IsExternal = isExternal,
        IsSizing = false
    };

    private User SeedUser(Guid? employeeId, UserStatus status = UserStatus.Active)
    {
        var user = new User
        {
            UserName = "70123456",
            PasswordHash = "hash",
            Email = "ana@test.com",
            FirstName = "Ana",
            LastName = "Quispe",
            Status = status,
            EmployeeId = employeeId
        };
        _context.Users.Add(user);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        return user;
    }

    /// <summary>Escribe la plantilla donde el código la busca ahora: junto al ejecutable.</summary>
    private static string WriteTemplateNextToExecutable()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Template", "Mail", "CreateUser.html");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "<p>{{ Username }} {{ Password }} <a href=\"{{ LoginUrl }}\">entrar</a></p>");
        return path;
    }

    [Fact]
    public async Task Create_CuandoFallaElCorreo_ElAltaSigueSiendoValida()
    {
        _emailService
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new FileNotFoundException("Could not find a part of the path 'Template\\Mail\\CreateUser.html'"));

        var result = await _userService.Create(NewRequest());

        result.IsValid.Should().BeTrue(because: "el usuario ya se guardó; un fallo de correo no puede invalidar el alta");
        result.Data.Should().NotBeNull();
        _context.Users.Should().ContainSingle(u => u.UserName == "70123456");
    }

    [Fact]
    public async Task Create_ResuelveLaPlantillaJuntoAlEjecutable_NoContraElDirectorioActual()
    {
        WriteTemplateNextToExecutable();

        // Un servicio de Windows arranca con el directorio actual en system32. Se simula moviendo
        // el directorio actual fuera de la carpeta del ejecutable: si la ruta se resolviera contra
        // él, no encontraría la plantilla y no se enviaría nada.
        var original = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(Path.GetTempPath());

            var result = await _userService.Create(NewRequest());

            result.IsValid.Should().BeTrue();
            _emailService.Verify(
                s => s.SendEmailAsync("ana@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Once);
        }
        finally
        {
            Directory.SetCurrentDirectory(original);
        }
    }

    [Fact]
    public async Task Create_BeneficiarioQuePasaAColaborador_EnlazaElEmpleadoSinDuplicar()
    {
        var existing = SeedUser(employeeId: null);
        var employeeId = Guid.NewGuid();

        var result = await _userService.Create(NewRequest(employeeId: employeeId, isExternal: true));

        result.IsValid.Should().BeTrue(because: "es la misma persona, ahora también colaborador");
        result.Data!.UserId.Should().Be(existing.UserId, because: "debe reutilizar el usuario existente, no crear otro");

        _context.Users.Should().ContainSingle(u => u.UserName == "70123456");
        var saved = _context.Users.Single(u => u.UserId == existing.UserId);
        saved.EmployeeId.Should().Be(employeeId);
        saved.IsExternal.Should().BeTrue();
    }

    [Fact]
    public async Task Create_BeneficiarioInactivoQuePasaAColaborador_SeReactiva()
    {
        var existing = SeedUser(employeeId: null, status: UserStatus.Inactive);

        var result = await _userService.Create(NewRequest(employeeId: Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
        _context.Users.Single(u => u.UserId == existing.UserId).Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Create_BeneficiarioQuePasaAColaborador_NoReenviaCorreoDeBienvenida()
    {
        SeedUser(employeeId: null);

        await _userService.Create(NewRequest(employeeId: Guid.NewGuid()));

        _emailService.Verify(
            s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never,
            "ya tiene credenciales de su alta anterior");
    }

    [Fact]
    public async Task Create_UsuarioDuplicadoRealmente_SigueRechazandose()
    {
        var employeeId = Guid.NewGuid();
        SeedUser(employeeId: employeeId);

        var result = await _userService.Create(NewRequest(employeeId: employeeId));

        result.IsValid.Should().BeFalse(because: "no debe romperse la validación de duplicados");
        _context.Users.Should().ContainSingle(u => u.UserName == "70123456");
    }

    // Las cuatro combinaciones de IsSizing x IsExternal tienen que enviar correo. Antes solo dos
    // tenían rama y los beneficiarios de ventas que no son de sizing (IsSizing=false +
    // IsExternal=true, las NG001 "A medida") salían sin credenciales y sin log.
    [Theory]
    [InlineData(true, true, PortalBeneficiario)]    // beneficiario, venta de sizing (NG002)
    [InlineData(false, true, PortalBeneficiario)]   // beneficiario, venta "A medida" (NG001)
    [InlineData(false, false, PanelAdmin)]          // colaborador
    [InlineData(true, false, PanelAdmin)]           // colaborador, venta de sizing
    public async Task Create_EnviaCorreoEnLasCuatroCombinaciones(bool isSizing, bool isExternal, string urlEsperada)
    {
        WriteTemplateNextToExecutable();

        string? cuerpoEnviado = null;
        _emailService
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Callback<string, string, string, bool>((_, _, body, _) => cuerpoEnviado = body)
            .Returns(Task.CompletedTask);

        var request = NewRequest(isExternal: isExternal);
        request.IsSizing = isSizing;

        var result = await _userService.Create(request);

        result.IsValid.Should().BeTrue();
        _emailService.Verify(
            s => s.SendEmailAsync("ana@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Once);
        cuerpoEnviado.Should().Contain(urlEsperada, because: "el enlace debe apuntar al portal que le toca");
    }

    [Fact]
    public async Task Create_SiFaltaLaUrlConfigurada_NoEnviaCorreoSilenciosamente()
    {
        var service = new UserService(
            new UserRepository(_context),
            new PasswordService(),
            new Mock<IMapper>().Object,
            _emailService.Object,
            Options.Create(new WebAppSettings()), // sin Url ni PortalInternoUrl
            NullLogger<UserService>.Instance,
            new Mock<IEventPublisher>().Object);

        var result = await service.Create(NewRequest(isExternal: true));

        result.IsValid.Should().BeTrue(because: "el alta sigue siendo válida");
        _emailService.Verify(
            s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }

    public void Dispose() => _context.Dispose();
}
