using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RabbitWebApp.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
        }

        public Task NewEmailAsync(string from, string to, string title)
        {
            _logger.LogInformation($"Title: {title}, From: {from}, To: {to}");
            return Task.CompletedTask;
        }
    }
}
