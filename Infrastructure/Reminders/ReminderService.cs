using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Npgsql;

using SleepBot.Core.Interfaces;
using SleepBot.Infrastructure.Data;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SleepBot.Infrastructure.Reminders;

public class ReminderService : IReminderService, IDisposable
{
    private readonly ITelegramBotClient _botClient;
    private readonly DbConnectionFactory _dbFactory;
    private Timer? _timer;

    public ReminderService(ITelegramBotClient botClient, DbConnectionFactory dbFactory)
    {
        _botClient = botClient;
        _dbFactory = dbFactory;
    }

    public async Task SetReminderAsync(long userId, TimeSpan remindTime, CancellationToken cancellationToken)
    {
        await using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        var sql = @"
            INSERT INTO Reminders (UserId, Time)
            VALUES (@userId, @time)
            ON CONFLICT (UserId) DO UPDATE
            SET Time = EXCLUDED.Time;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("time", remindTime);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task RemoveRemindersAsync(long userId, CancellationToken cancellationToken)
    {
        await using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        var sql = "DELETE FROM Reminders WHERE UserId = @userId;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(async _ => await CheckRemindersAsync(cancellationToken), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }

    private async Task CheckRemindersAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.Now.TimeOfDay; // Локальное время
        var reminders = await GetAllRemindersAsync(cancellationToken);

        foreach (var (userId, remindTime) in reminders)
        {
            if (IsTimeToRemind(remindTime, now))
            {
                try
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: userId,
                        text: $"Уже {remindTime:hh\\:mm}! Пора готовиться ко сну. Спокойной ночи!",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
                catch
                {
                    // TODO: логирование ошибок
                }
            }
        }
    }

    private async Task<List<(long UserId, TimeSpan RemindTime)>> GetAllRemindersAsync(CancellationToken cancellationToken)
    {
        var results = new List<(long, TimeSpan)>();

        await using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        var sql = "SELECT UserId, Time FROM Reminders;";
        await using var cmd = new NpgsqlCommand(sql, conn);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var userId = reader.GetInt64(0);
            var remindTime = reader.GetTimeSpan(1);
            results.Add((userId, remindTime));
        }

        return results;
    }

    public async Task<TimeSpan?> GetByChatIdAsync(long userId, CancellationToken cancellationToken)
    {
        await using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        var sql = "SELECT Time FROM Reminders WHERE UserId = @userId LIMIT 1;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);

        if (result == null || result == DBNull.Value)
            return null;

        return (TimeSpan)result;
    }

    private static bool IsTimeToRemind(TimeSpan remindTime, TimeSpan now)
    {
        var diff = (now - remindTime).TotalMinutes;
        return diff >= 0 && diff < 1;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
