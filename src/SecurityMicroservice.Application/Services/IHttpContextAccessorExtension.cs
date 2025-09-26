using Microsoft.AspNetCore.Http;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Shared.Response.User;

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




            return response;
        }
    }
}
