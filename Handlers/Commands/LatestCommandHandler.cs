using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using SleepBot.Core.Interfaces;

namespace SleepBot.Handlers.Commands
{
    public class LatestCommandHandler : ICommandHandler
    {
        public string Command => "latest";

        private readonly ITelegramBotClient _botClient;
        private readonly ISleepService _sleepService;

        public LatestCommandHandler(ITelegramBotClient botClient, ISleepService sleepService)
        {
            _botClient = botClient;
            _sleepService = sleepService;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var session = await _sleepService.GetLatestSessionAsync(message.From!.Id, cancellationToken);

            if (session == null)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "У тебя пока нет записей о сне 😴",
                    cancellationToken: cancellationToken);
                return;
            }

            var duration = session.Duration;
            var response = $"🛌 Последний сон:\n" +
                           $"Начало: {session.SleepTime:dd.MM.yyyy HH:mm}\n" +
                           $"Конец: {session.WakeTime:dd.MM.yyyy HH:mm}\n" +
                           $"⏱ Длительность: {Math.Floor(duration.TotalHours)} ч {duration.Minutes} мин";

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                cancellationToken: cancellationToken);
        }
    }
}
