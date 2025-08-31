using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot.Types;

namespace SleepBot.Core.Interfaces
{
    public interface ICommandHandler
    {
        string Command { get; }
        Task HandleAsync(Message message, CancellationToken cancellationToken);
    }

}
