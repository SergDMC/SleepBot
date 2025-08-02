using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using SleepBot.Core.Entities;
using SleepBot.Core.DTO;

namespace SleepBot.Infrastructure.Data;

public class SleepRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SleepRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertSleepSessionAsync(SleepSession session, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO sleep_sessions (user_id, sleep_time, wake_time, duration)
            VALUES (@user_id, @sleep_time, @wake_time, @duration);
        ";

        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", session.UserId);
        cmd.Parameters.AddWithValue("sleep_time", session.SleepTime);
        cmd.Parameters.AddWithValue("wake_time", session.WakeTime);
        cmd.Parameters.AddWithValue("duration", session.Duration);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<SleepStatsDto?> GetSleepStatsForLastWeekAsync(long userId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                AVG(duration) as avg_duration,
                MIN(duration) as min_duration,
                MAX(duration) as max_duration
            FROM sleep_sessions
            WHERE user_id = @user_id
              AND sleep_time >= NOW() - INTERVAL '7 days';
        ";

        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        if (reader.IsDBNull(0)) // avg_duration is null
            return null;

        var avgDuration = reader.GetTimeSpan(0);
        var minDuration = reader.GetTimeSpan(1);
        var maxDuration = reader.GetTimeSpan(2);

        return new SleepStatsDto
        {
            AverageDuration = avgDuration,
            ShortestDuration = minDuration,
            LongestDuration = maxDuration
        };
    }
}
