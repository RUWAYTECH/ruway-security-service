using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using Xunit;

namespace SecurityMicroservice.UnitTests.Services;

public class AuthenticationServiceTests : IDisposable
{
    private readonly SecurityDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SecurityDbContext(options);
        _passwordService = new PasswordService();
        _userRepository = new UserRepository(_context);
        _authenticationService = new AuthenticationService(_userRepository, _passwordService);
    }

    [Fact]
    public async Task ValidateUserAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            PasswordHash = _passwordService.HashPassword("password123"),
            Status = UserStatus.Active
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authenticationService.ValidateUserAsync("testuser", "password123");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task ValidateUserAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            PasswordHash = _passwordService.HashPassword("password123"),
            Status = UserStatus.Active
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authenticationService.ValidateUserAsync("testuser", "wrongpassword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateUserAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            PasswordHash = _passwordService.HashPassword("password123"),
            Status = UserStatus.Inactive
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authenticationService.ValidateUserAsync("testuser", "password123");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GenerateTokenResponseAsync_ShouldIncludeUserClaims()
    {
        // Arrange
        var app = new Application
        {
            Code = "AUDITORIA",
            Name = "Test App",
            BaseUrl = "https://test.com"
        };

        var role = new Role
        {
            ApplicationId = app.ApplicationId,
            Code = "ADMIN",
            Name = "Administrator"
        };

        var user = new User
        {
            Username = "testuser",
            PasswordHash = _passwordService.HashPassword("password123"),
            Status = UserStatus.Active,
            EmployeeId = Guid.NewGuid()
        };

        var userApp = new UserApplication
        {
            UserId = user.UserId,
            ApplicationId = app.ApplicationId,
            IsActive = true
        };

        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = role.RoleId
        };

        _context.Applications.Add(app);
        _context.Roles.Add(role);
        _context.Users.Add(user);
        _context.UserApplications.Add(userApp);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authenticationService.GenerateTokenResponseAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.EmployeeId.Should().Be(user.EmployeeId);
        result.Scope.Should().Contain("auditoria");
        result.Roles.Should().Contain("AUDITORIA_ADMIN");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}