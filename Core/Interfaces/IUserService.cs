using System.Threading;
using System.Threading.Tasks;

namespace SleepBot.Core.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(long chatId, CancellationToken cancellationToken);
    }

}
