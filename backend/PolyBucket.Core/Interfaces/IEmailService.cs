using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    }
} 