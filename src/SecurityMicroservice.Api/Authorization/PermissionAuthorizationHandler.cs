using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SecurityMicroservice.Api.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();
        
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}

public static class AuthorizationExtensions
{
    public static void AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("Permission:AUDITORIA:EXPEDIENTES:GET", policy =>
                policy.Requirements.Add(new PermissionRequirement("AUDITORIA:EXPEDIENTES:READ")))
            .AddPolicy("Permission:AUDITORIA:EXPEDIENTES:POST", policy =>
                policy.Requirements.Add(new PermissionRequirement("AUDITORIA:EXPEDIENTES:CREATE")))
            .AddPolicy("Permission:MEMOS:DOCUMENTOS:GET", policy =>
                policy.Requirements.Add(new PermissionRequirement("MEMOS:DOCUMENTOS:READ")));
    }
}