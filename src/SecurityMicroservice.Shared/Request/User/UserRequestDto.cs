namespace SecurityMicroservice.Shared.Request.User
{
    public class UserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Status { get; set; }
        public bool? IsExternal { get; set; }
        public Guid? EmployeeId { get; set; }
    }
}
