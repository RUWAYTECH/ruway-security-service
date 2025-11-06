using Rokys.Memo.Common.Constant;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Shared.Response.User
{
    public class UserCurrentResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<ApplicationDto> Applications { get; set; } = new();
        public List<RoleDto> Roles { get; set; } = new();

        public bool IsSuperAdmin { get { return Roles.Any(r => r.Code == RoleCodes.SuperAdmin); } }
        public bool IsAppAdmin { get { return Roles.Any(r => r.Code == RoleCodes.ApplicationAdmin); } }
    }
}
