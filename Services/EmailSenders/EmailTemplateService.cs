using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Services.EmailSenders
{
    public interface IEmailTemplateService
    {
        Task<string> GetWelcomeEmailTemplateAsync(string customerFullName, string username, string phoneNumber, string email, string password);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _environment;

        public EmailTemplateService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> GetWelcomeEmailTemplateAsync(string customerFullName, string username, string phoneNumber, string email, string password)
        {
            var templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "WelcomeEmail.html");
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found at: {templatePath}");
            }

            var template = await File.ReadAllTextAsync(templatePath);

            // Replace placeholders with actual values
            template = template.Replace("{{CustomerFullName}}", customerFullName);
            template = template.Replace("{{Username}}", username);
            template = template.Replace("{{PhoneNumber}}", phoneNumber);
            template = template.Replace("{{Email}}", email);
            template = template.Replace("{{Password}}", password);
            template = template.Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            return template;
        }
    }
}
