using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using SleepBot.Handlers;
using SleepBot.Core.Interfaces;
using Telegram.Bot.Types;


namespace SleepBot.Bot;

/// <summary>
/// Фоновый сервис для запуска и поддержки работы Telegram-бота.
/// </summary>
public class SleepBotWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private TelegramBotClient? _botClient;

    public SleepBotWorker(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Переменная окружения TELEGRAM_BOT_TOKEN не установлена.");

        _botClient = new TelegramBotClient(token);

        var handler = new UpdateHandler(_services);

        _botClient.StartReceiving(
            updateHandler: handler.HandleUpdateAsync,
            pollingErrorHandler: handler.HandleErrorAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Все типы обновлений
            },
            cancellationToken: stoppingToken);

        var me = await _botClient.GetMeAsync(stoppingToken);
        Console.WriteLine($"Бот запущен: @{me.Username}");

        await _botClient.SetMyCommandsAsync(new[]
    {
        new BotCommand { Command = "help", Description = "Показать информацию о доступных командах" },
        new BotCommand { Command = "sleep", Description = "Записать сон" },
        new BotCommand { Command = "stats", Description = "Показать статистику" },
        new BotCommand { Command = "remind", Description = "Настроить напоминания" },
    }, cancellationToken: stoppingToken);


        // Запускаем сервис напоминаний
        var reminderService = _services.GetRequiredService<IReminderService>();
        await reminderService.StartAsync(stoppingToken);

        // Ожидаем отмены задачи
        await Task.Delay(Timeout.Infinite, stoppingToken);

        // При остановке
        await reminderService.StopAsync(stoppingToken);
    }
}
