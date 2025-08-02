using System;
using System.Threading;
using System.Threading.Tasks;

namespace SleepBot.Core.Interfaces;

public interface IReminderService
{
    /// <summary>
    /// Устанавливает напоминание для пользователя.
    /// Время передается в формате HH:mm.
    /// </summary>
    Task SetReminderAsync(long userId, TimeSpan remindTime, CancellationToken cancellationToken);

    /// <summary>
    /// Запускает фоновую задачу, которая проверяет и отправляет напоминания.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Останавливает фоновую задачу.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);
}
