using System.Threading;
using System.Threading.Tasks;
using SleepBot.Core.Entities;
using SleepBot.Core.DTO;

namespace SleepBot.Core.Interfaces;

public interface ISleepService
{
   
    /// Сохраняет сессию сна пользователя.
    
    Task SaveSessionAsync(SleepSession session, CancellationToken cancellationToken);

    
    /// Получает статистику сна за последнюю неделю.
    
    Task<SleepStatsDto?> GetSleepStatsAsync(long userId, CancellationToken cancellationToken);

    Task<SleepSession?> GetLatestSessionAsync(long userId, CancellationToken cancellationToken);
}
