namespace SecurityMicroservice.Application.IServices;

public interface IRecaptchaService
{
    Task<(bool IsValid, double Score)> ValidateTokenAsync(string token, string? remoteIp = null);
}
