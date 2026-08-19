using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Ruway.Events.Command.Interfaces.Events;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Application.Services.Emails;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;
using Xunit;

namespace SecurityMicroservice.UnitTests.Services;

/// <summary>
/// Los beneficiarios (PORTALCLIENTE) no tienen EmployeeId: la publicación de eventos no puede
/// asumir que siempre hay uno.
/// </summary>
public class UserServiceEventPublishingTests : IDisposable
{
    private readonly SecurityDbContext _context;
    private readonly UserService _userService;
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly List<IDomainEvent> _publishedEvents = new();

    public UserServiceEventPublishingTests()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SecurityDbContext(options);

        _eventPublisher
            .Setup(p => p.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IDomainEvent, CancellationToken>((e, _) => _publishedEvents.Add(e))
            .Returns(Task.CompletedTask);

        _userService = new UserService(
            new UserRepository(_context),
            new PasswordService(),
            new Mock<IMapper>().Object,
            new Mock<IEmailService>().Object,
            Options.Create(new WebAppSettings()),
            NullLogger<UserService>.Instance,
            _eventPublisher.Object);
    }

    private User AddUser(Guid? employeeId)
    {
        var user = new User
        {
            UserName = "70123456",
            PasswordHash = "hash",
            Email = "beneficiario@test.com",
            FirstName = "Ana",
            LastName = "Quispe",
            Status = UserStatus.Active,
            EmployeeId = employeeId
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        // El servicio recibe un contexto recién creado en cada request; sin limpiar el tracker la
        // instancia sembrada aquí choca con la que devuelve el repositorio.
        _context.ChangeTracker.Clear();
        return user;
    }

    [Fact]
    public async Task Delete_CuandoEmployeeIdEsNull_DesactivaSinLanzarNRE()
    {
        var user = AddUser(employeeId: null);

        var result = await _userService.Delete(user.UserId);

        result.IsValid.Should().BeTrue(because: "un beneficiario sin EmployeeId debe poder desactivarse");
        _context.Users.Single(u => u.UserId == user.UserId).Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Delete_CuandoEmployeeIdEsNull_PublicaEventoConEmployeeIdVacio()
    {
        var user = AddUser(employeeId: null);

        await _userService.Delete(user.UserId);

        var published = _publishedEvents.OfType<UserUpdatedEvent>().Single();
        published.UserId.Should().Be(user.UserId);
        published.EmployeeId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task Delete_CuandoElBrokerFalla_NoConvierteLaBajaEnError()
    {
        var user = AddUser(employeeId: null);
        _eventPublisher
            .Setup(p => p.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("broker caído"));

        var result = await _userService.Delete(user.UserId);

        result.IsValid.Should().BeTrue(because: "el cambio ya se persistió; un broker caído no debe reportarse como fallo de la operación");
        _context.Users.Single(u => u.UserId == user.UserId).Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task UpdatePartial_CuandoEmployeeIdEsNull_NoLanzaNRE()
    {
        var user = AddUser(employeeId: null);

        var result = await _userService.UpdatePartial(
            user.UserId,
            new UserRequestDto { FirstName = "Ana María", Status = "Active" },
            password: null);

        result.IsValid.Should().BeTrue(because: "la ruta de PeopleUpdated para beneficiarios pasa por aquí");
        _context.Users.Single(u => u.UserId == user.UserId).FirstName.Should().Be("Ana María");
    }

    public void Dispose() => _context.Dispose();
}
