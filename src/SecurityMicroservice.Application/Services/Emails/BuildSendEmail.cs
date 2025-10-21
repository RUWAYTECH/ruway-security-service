using Scriban;
using SecurityMicroservice.Application.IServices;
using static Rokys.Memo.Common.Constant.Constants;

namespace SecurityMicroservice.Application.Services.Emails
{
    public class BuildSendEmail
    {
         public static async Task ResetPasswordEmail(IEmailService emailService, string email, string firstName, string token, string urlApp)
        {
            var inputTexts = new Dictionary<string, object>
            {
                ["EmployeeFullName"] = firstName,
                ["ResetPasswordUrl"] = $"{urlApp}/reset-password?token={token}"
            };

            var templateText = File.ReadAllText(MailTemplate.ResetPassword);
            var template = Template.Parse(templateText);
            var htmlBody = template.Render(inputTexts);

            if (!string.IsNullOrEmpty(email))
                await emailService.SendEmailAsync(email, "Restablecer contraseña", htmlBody, true);
        }
    }
}