using System.Threading;
using System.Threading.Tasks;
using SleepBot.Core.Entities;
using SleepBot.Core.Interfaces;
using SleepBot.Core.DTO;
using SleepBot.Infrastructure.Data;

namespace SleepBot.Infrastructure.Services;

public class SleepService : ISleepService
{
    private readonly SleepRepository _repository;

    public SleepService(SleepRepository repository)
    {
        _repository = repository;
    }

    public async Task SaveSessionAsync(SleepSession session, CancellationToken cancellationToken)
    {
        await _repository.InsertSleepSessionAsync(session, cancellationToken);
    }

    public async Task<SleepStatsDto?> GetSleepStatsAsync(long userId, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetSleepStatsForLastWeekAsync(userId, cancellationToken);
        return stats;
    }

    public Task<SleepSession?> GetLatestSessionAsync(long userId, CancellationToken cancellationToken)
        => _repository.GetLatestSessionAsync(userId, cancellationToken);
}
