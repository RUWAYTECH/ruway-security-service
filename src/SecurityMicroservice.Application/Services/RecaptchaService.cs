using Microsoft.Extensions.Configuration;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Response.Recaptcha;
using System.Text.Json;

namespace SecurityMicroservice.Application.Services;

public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private const string RecaptchaUrl = "https://www.google.com/recaptcha/api/siteverify";

    public RecaptchaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Recaptcha:SecretKey"] ?? throw new ArgumentNullException("Recaptcha:SecretKey not configured");
    }

    public async Task<(bool IsValid, double Score)> ValidateTokenAsync(string token, string? remoteIp = null)
    {
        try
        {
            var parameters = new List<KeyValuePair<string, string>>
                {
                    new("secret", _secretKey),
                    new("response", token)
                };

            if (!string.IsNullOrEmpty(remoteIp))
            {
                parameters.Add(new("remoteip", remoteIp));
            }

            var formContent = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(RecaptchaUrl, formContent);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponseDto>(jsonResponse);

            if (recaptchaResponse?.Success != true)
            {
                return (false, 0.0);
            }

            // Para reCAPTCHA v3, verificar el score (0.0 - 1.0)
            // Valores más altos indican menor probabilidad de ser bot
            return (recaptchaResponse.Score >= 0, recaptchaResponse.Score);
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error validating reCAPTCHA: {ex.Message}");
            return (false, 0.0);
        }
    }
}
