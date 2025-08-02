using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using SleepBot.Core.Interfaces;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SleepBot.Infrastructure.Reminders;

public class ReminderService : IReminderService, IDisposable
{
    private readonly ITelegramBotClient _botClient;
    private readonly ConcurrentDictionary<long, TimeSpan> _userReminders = new();
    private Timer? _timer;

    public ReminderService(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public Task SetReminderAsync(long userId, TimeSpan remindTime, CancellationToken cancellationToken)
    {
        _userReminders.AddOrUpdate(userId, remindTime, (_, _) => remindTime);
        return Task.CompletedTask;
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
        var nowUtc = DateTime.UtcNow.TimeOfDay;

        foreach (var (userId, remindTime) in _userReminders)
        {
            // Разрешаем "окно" в 1 минуту для срабатывания напоминания
            if (IsTimeToRemind(remindTime, nowUtc))
            {
                try
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: userId,
                        text: $"Уже {remindTime:hh\\:mm}! Пора готовиться ко сну, чтобы выспаться. Спокойной ночи!",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
                catch
                {
                    // Логирование ошибок — можно расширить
                }
            }
        }
    }

    private static bool IsTimeToRemind(TimeSpan remindTime, TimeSpan now)
    {
        var diff = (now - remindTime).TotalMinutes;
        return diff >= 0 && diff < 1; // срабатывает в течение 1 минуты после времени
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
