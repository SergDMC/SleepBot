using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using SleepBot.Core.Interfaces;

namespace SleepBot.Handlers
{
    public class StatsCommandHandler : ICommandHandler
    {
        public string Command => "stats";

        private readonly ITelegramBotClient _botClient;
        private readonly ISleepService _sleepService;

        public StatsCommandHandler(ITelegramBotClient botClient, ISleepService sleepService)
        {
            _botClient = botClient;
            _sleepService = sleepService;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var stats = await _sleepService.GetSleepStatsAsync(message.From!.Id, cancellationToken);

            if (stats == null || stats.AverageDuration.TotalMinutes == 0)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Нет данных за последнюю неделю.",
                    cancellationToken: cancellationToken);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Статистика сна за последнюю неделю:");
            sb.AppendLine($"- Средняя продолжительность: {stats.AverageDuration.Hours} ч {stats.AverageDuration.Minutes} мин");
            sb.AppendLine($"- Самая короткая ночь: {stats.ShortestDuration.Hours} ч {stats.ShortestDuration.Minutes} мин");
            sb.AppendLine($"- Самая длинная ночь: {stats.LongestDuration.Hours} ч {stats.LongestDuration.Minutes} мин");
            sb.AppendLine();
            sb.AppendLine("Совет: старайся спать не менее 7 часов в день!");

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: sb.ToString(),
                cancellationToken: cancellationToken);
        }
    }
}
