using SleepBot.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot.Types;

using Telegram.Bot;

namespace SleepBot.Handlers
{
    public class TargetCommandHandler : ICommandHandler
    {
        public string Command => "target";

        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;

        public TargetCommandHandler(ITelegramBotClient botClient, IUserService userService)
        {
            _botClient = botClient;
            _userService = userService;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var args = message.Text?.Replace("/target", "").Trim();

            if (string.IsNullOrEmpty(args))
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "❌ Укажите цель в часах, например: `/target 8`",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
                return;
            }

            if (int.TryParse(args, out int targetHours) && targetHours > 0)
            {
                await _userService.SetTargetAsync(message.Chat.Id, targetHours, cancellationToken);

                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"✅ Цель по сну установлена: {targetHours} ч.",
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "❌ Неверный формат. Укажите число часов, например: `/target 8`",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
        }
    }

}
