using SleepBot.Core.Interfaces;


using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace SleepBot.Handlers
{
    public class HelpCommandHandler : ICommandHandler
    {
        public string Command => "help";

        private readonly ITelegramBotClient _botClient;
        

        public HelpCommandHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
            
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var helpText = """
        💤 *SleepBot* – бот для отслеживания сна

        Доступные команды:

        /sleep – зафиксировать продолжительность сна, используй формат команды: /sleep 23:30 07:00

        /stats – статистика сна за последние 7 дней

        /remind – включить напоминания о времени отхода ко сну, используй формат команды: /remind 23:00

        /help – показать эту справку

        *Совет*: Старайтесь спать не менее 7 часов в сутки!
        """;

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: helpText,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
    }
}
