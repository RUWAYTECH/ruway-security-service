namespace SecurityMicroservice.Shared.Request.User
{
    public class UserRequestDto: BaseUserRequestDto
    {       
        public string Password { get; set; } = string.Empty;
        public bool? IsSizing { get; set; }
    }
}
