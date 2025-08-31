using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using SleepBot.Bot;
using SleepBot.Core.Interfaces;
using SleepBot.Infrastructure.Data;
using SleepBot.Infrastructure.Reminders;
using SleepBot.Infrastructure.Services;
using System;
using SleepBot.Handlers.Commands;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Переменная окружения TELEGRAM_BOT_TOKEN не установлена.");

        // Telegram client
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));

        // Infrastructure
        services.AddSingleton<DbConnectionFactory>();
        services.AddScoped<SleepRepository, SleepRepository>();
        services.AddScoped<ISleepService, SleepService>();
        services.AddSingleton<IReminderService, ReminderService>();
        services.AddScoped<IUserService, UserService>();

        // Command Handlers
        services.AddSingleton<ICommandHandler, StartCommandHandler>();
        services.AddScoped<ICommandHandler, SleepCommandHandler>();
        services.AddScoped<ICommandHandler, StatsCommandHandler>();
        services.AddScoped<ICommandHandler, RemindCommandHandler>();
        services.AddScoped<ICommandHandler, HelpCommandHandler>();
        services.AddScoped<ICommandHandler, TargetCommandHandler>();
        services.AddScoped<ICommandHandler, SettingsCommandHandler>();
        services.AddScoped<ICommandHandler, LatestCommandHandler>();


        // Background worker
        services.AddHostedService<SleepBotWorker>();
    })
    .Build();

await host.RunAsync();
