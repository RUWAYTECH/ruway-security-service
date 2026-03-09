namespace SecurityMicroservice.Shared.Request.User
{
    public class BaseUserRequestDto
    {
        public Guid? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Status { get; set; }
        public bool? IsExternal { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? RoleCode { get; set; }
    }
}
