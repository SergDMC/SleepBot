using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SleepBot.Core.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SleepBot.Handlers.Commands
{
    public class SettingsCommandHandler : ICommandHandler
    {
        public string Command => "settings";

        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IReminderService _reminderService;

        public SettingsCommandHandler(
            ITelegramBotClient botClient,
            IUserService userService,
            IReminderService reminderService)
        {
            _botClient = botClient;
            _userService = userService;
            _reminderService = reminderService;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;

            // Получаем пользователя
            var user = await _userService.GetByChatIdAsync(chatId, cancellationToken);
            if (user == null)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Ты ещё не зарегистрирован. Используй `/start` для начала работы с ботом.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
                return;
            }

            // Получаем напоминание, если есть
            var reminder = await _reminderService.GetByChatIdAsync(chatId, cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine("⚙️ Твои настройки\n");

            // Профиль
            sb.AppendLine("👤 Профиль");
            sb.AppendLine($"• Имя: @{user.Username ?? "не задано"}");
            sb.AppendLine($"• Дата присоединения: {user.CreatedAt:yyyy-MM-dd}\n");

            // Цели по сну
            sb.AppendLine("🎯 Цели по сну");
            sb.AppendLine($"• Текущая цель: {(user.TargetHours.HasValue ? user.TargetHours.Value + " часов" : "не задано")}\n");

            // Напоминания
            sb.AppendLine("⏰ Напоминания");
            sb.AppendLine($"• Напоминание о времени отхода ко сну: {(reminder.HasValue ? reminder.Value.ToString(@"hh\:mm") + " ✅" : "не установлено")}\n");

            // Команды для обновления
            sb.AppendLine("📝 Команды для обновления настроек:");
            sb.AppendLine("• `/target 8` - Установить цель по сну");
            sb.AppendLine("• `/remind 23:00` - Установить напоминание");
            sb.AppendLine("• `/remind off` - Отключить напоминание");

            await _botClient.SendTextMessageAsync(
                chatId,
                sb.ToString(),
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
    }
}
