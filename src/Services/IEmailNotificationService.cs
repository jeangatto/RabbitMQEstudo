using System.Threading.Tasks;

namespace RabbitWebApp.Services
{
    public interface IEmailNotificationService
    {
        Task NewEmailAsync(string from, string to, string title);
    }
}
