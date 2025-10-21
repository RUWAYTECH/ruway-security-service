using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityMicroservice.Application.IServices
{
    public interface IEmailService
    {        
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}