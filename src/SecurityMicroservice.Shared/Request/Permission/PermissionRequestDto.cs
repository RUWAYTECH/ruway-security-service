namespace SecurityMicroservice.Shared.Request.Permission
{
    public class PermissionRequestDto
    {
        public Guid RoleId { get; set; }
        public Guid OptionId { get; set; }
        public string ActionCode { get; set; } = string.Empty;
    }
}
