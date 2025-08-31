using System.Threading;
using System.Threading.Tasks;
using SleepBot.Core.Entities;

namespace SleepBot.Core.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken);
        Task RegisterUserAsync(long chatId, CancellationToken cancellationToken);
        Task SetTargetAsync(long chatId, int targetHours, CancellationToken cancellationToken);

    }

}
