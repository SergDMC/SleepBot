using System.Threading;
using System.Threading.Tasks;
using SleepBot.Core.Entities;
using SleepBot.Core.DTO;

namespace SleepBot.Core.Interfaces;

public interface ISleepService
{
    /// <summary>
    /// Сохраняет сессию сна пользователя.
    /// </summary>
    Task SaveSessionAsync(SleepSession session, CancellationToken cancellationToken);

    /// <summary>
    /// Получает статистику сна за последнюю неделю.
    /// </summary>
    Task<SleepStatsDto?> GetSleepStatsAsync(long userId, CancellationToken cancellationToken);
}
