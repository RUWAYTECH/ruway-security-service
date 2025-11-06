using Microsoft.AspNetCore.Http;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Shared.Response.User;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Application.Services
{
    public static class IHttpContextAccessorExtension
    {
        public static UserCurrentResponseDto CurrentUser(this IHttpContextAccessor httpContextAccessor)
        {
            var userName = httpContextAccessor?.HttpContext?.User?.FindFirst(Constants.ClaimNames.NameId)?.Value;
            var response = new UserCurrentResponseDto { Username = userName };

            var first_name = httpContextAccessor?.HttpContext?.User?.FindFirst(Constants.ClaimNames.FirstName)?.Value;
            var last_name = httpContextAccessor?.HttpContext?.User?.FindFirst(Constants.ClaimNames.LastName)?.Value;

            response.Username = $"{first_name} {last_name}";

            var employeeId = httpContextAccessor?.HttpContext?.User?.FindFirst(Constants.ClaimNames.EmployeeId)?.Value;
            Guid.TryParse(employeeId, out Guid outEmployeeId);
            response.EmployeeId = outEmployeeId;


            var userId = httpContextAccessor?.HttpContext?.User?.FindFirst(Constants.ClaimNames.UserId)?.Value;
            Guid.TryParse(userId, out Guid outUserId);
            response.UserId = outUserId;

            response.Roles = httpContextAccessor?.HttpContext?.User?.FindAll(Constants.ClaimNames.Roles)
                ?.Select(r => ParseRoleFromClaim(r.Value))
                .Where(role => role != null)
                .Cast<RoleDto>()
                .ToList() ?? new List<RoleDto>();

            return response;
        }

        private static RoleDto? ParseRoleFromClaim(string roleClaim)
        {
            try
            {
                // Format: "CODIGO_APLICACION_CODIGO_ROL:NOMBRE_ROL"
                // Example: "MEMOS_APPADMIN:Administrador de aplicación"
                
                var parts = roleClaim.Split(':');
                if (parts.Length != 2) return null;

                var roleCodePart = parts[0]; // "MEMOS_APPADMIN"
                var roleName = parts[1]; // "Administrador de aplicación"

                // Split the role code part to get application code and role code
                var roleCodeSegments = roleCodePart.Split('_');
                if (roleCodeSegments.Length < 2) return null;

                var applicationCode = roleCodeSegments[0]; // "MEMOS"
                var roleCode = string.Join("_", roleCodeSegments.Skip(1)); // "APPADMIN" or "SUPER_ADMIN"

                return new RoleDto
                {
                    ApplicationCode = applicationCode,
                    Code = roleCode,
                    Name = roleName,
                    IsActive = true
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
