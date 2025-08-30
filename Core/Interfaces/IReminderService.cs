using SleepBot.Infrastructure.Reminders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SleepBot.Core.Interfaces;

public interface IReminderService
{
   
    // Устанавливает напоминание для пользователя.
    // Время передается в формате HH:mm.
    
    Task SetReminderAsync(long userId, TimeSpan remindTime, CancellationToken cancellationToken);

    // Удалить текущее напоминание о времени отхода ко сну
    Task RemoveRemindersAsync(long userid,  CancellationToken cancellationToken);


    // Запускает фоновую задачу, которая проверяет и отправляет напоминания.

    Task StartAsync(CancellationToken cancellationToken);

    Task<TimeSpan?> GetByChatIdAsync(long userId, CancellationToken cancellationToken);


    // Останавливает фоновую задачу.

    Task StopAsync(CancellationToken cancellationToken);
}
