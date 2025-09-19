using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using SecurityMicroservice.Api.Authorization;
using System.Security.Claims;
using Xunit;

namespace SecurityMicroservice.UnitTests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WithMatchingPermission_ShouldSucceed()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement("AUDITORIA:EXPEDIENTES:READ");
        
        var claims = new[]
        {
            new Claim("permissions", "AUDITORIA:EXPEDIENTES:READ"),
            new Claim("permissions", "MEMOS:DOCUMENTOS:READ")
        };
        
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithoutMatchingPermission_ShouldNotSucceed()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement("AUDITORIA:EXPEDIENTES:WRITE");
        
        var claims = new[]
        {
            new Claim("permissions", "AUDITORIA:EXPEDIENTES:READ"),
            new Claim("permissions", "MEMOS:DOCUMENTOS:READ")
        };
        
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoPermissions_ShouldNotSucceed()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement("AUDITORIA:EXPEDIENTES:READ");
        
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}