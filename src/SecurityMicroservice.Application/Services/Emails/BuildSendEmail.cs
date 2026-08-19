using Scriban;
using SecurityMicroservice.Application.IServices;
using static Rokys.Memo.Common.Constant.Constants;

namespace SecurityMicroservice.Application.Services.Emails
{
    public class BuildSendEmail
    {
        /// <summary>
        /// Ancla la ruta de la plantilla a la carpeta del ejecutable. Un servicio de Windows
        /// arranca con el directorio actual en C:\Windows\system32, así que una ruta relativa
        /// no resuelve al desplegar el Subscription Hub.
        /// </summary>
        private static string ResolveTemplatePath(string relativePath)
        {
            var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(AppContext.BaseDirectory, normalized);
        }

        public static async Task ResetPasswordEmail(IEmailService emailService, string email, string firstName, string token, string urlApp, string? applicationCode)
        {
            var inputTexts = new Dictionary<string, object>
            {
                ["EmployeeFullName"] = firstName,
                ["ResetPasswordUrl"] = $"{urlApp}/reset-password?token={token}"
            };

            var templateText = File.ReadAllText(ResolveTemplatePath(MailTemplate.ResetPassword));
            var template = Template.Parse(templateText);
            var htmlBody = template.Render(inputTexts);

            if (!string.IsNullOrEmpty(email))
                await emailService.SendEmailAsync(email, "Restablecer contraseña", htmlBody, true);
        }
        public static async Task CreateUserEmail(IEmailService emailService, string email, string firstName, string lastName, string username, string password, string urlApp)
        {
            var inputTexts = new Dictionary<string, object>
            {
                ["EmployeeFullName"] = firstName + " " + lastName,
                ["Username"] = username,
                ["Password"] = password,
                ["LoginUrl"] = $"{urlApp}"
            };

            var templateText = File.ReadAllText(ResolveTemplatePath(MailTemplate.CreateUser));
            var template = Template.Parse(templateText);
            var htmlBody = template.Render(inputTexts);

            if (!string.IsNullOrEmpty(email))
                await emailService.SendEmailAsync(email, "Cuenta creada", htmlBody, true);
        }
    }
}