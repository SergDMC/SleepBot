using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using SleepBot.Core.Interfaces;
using SleepBot.Core.Entities;

namespace SleepBot.Handlers
{
    public class SleepCommandHandler : ICommandHandler
    {
        public string Command => "sleep";

        private readonly ITelegramBotClient _botClient;
        private readonly ISleepService _sleepService;

        public SleepCommandHandler(ITelegramBotClient botClient, ISleepService sleepService)
        {
            _botClient = botClient;
            _sleepService = sleepService;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var args = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 3)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Неверный формат команды. Используй: `/sleep 23:30 07:00`",
                    cancellationToken: cancellationToken);
                return;
            }

            if (!TimeSpan.TryParseExact(args[1], @"hh\:mm", CultureInfo.InvariantCulture, out var sleepTime) ||
                !TimeSpan.TryParseExact(args[2], @"hh\:mm", CultureInfo.InvariantCulture, out var wakeTime))
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Время должно быть в формате HH:mm. Пример:`/sleep 23:00 07:30`",
                    cancellationToken: cancellationToken);
                return;
            }

            var today = DateTime.UtcNow.Date;
            var sleepDate = today.AddDays(wakeTime < sleepTime ? -1 : 0).Add(sleepTime);
            var wakeDate = today.Add(wakeTime);
            var duration = wakeDate - sleepDate;

            var session = new SleepSession
            {
                UserId = message.From!.Id,
                SleepTime = sleepDate,
                WakeTime = wakeDate,
                Duration = duration
            };

            await _sleepService.SaveSessionAsync(session, cancellationToken);

            var response = $"Записал! Ты спал {Math.Floor(duration.TotalHours)} ч {duration.Minutes} мин. Отличный результат!";
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                cancellationToken: cancellationToken);
        }
    }
}
