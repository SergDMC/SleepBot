using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using SleepBot.Core.Interfaces;

namespace SleepBot.Handlers
{
    public class RemindCommandHandler : ICommandHandler
    {
        public string Command => "remind";

        private readonly ITelegramBotClient _botClient;
        private readonly IReminderService _reminderService;

        public RemindCommandHandler(ITelegramBotClient botClient, IReminderService reminderService)
        {
            _botClient = botClient;
            _reminderService = reminderService;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var args = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Нет аргументов — сразу ошибка
            if (args.Length != 2)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Неверный формат. Используй: `/remind 23:00` или `/remind off`",
                    cancellationToken: cancellationToken);
                return;
            }

            // Проверяем, что передали "off"
            if (args[1].Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                await _reminderService.RemoveRemindersAsync(message.From!.Id, cancellationToken);

                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Напоминания отключены ✅",
                    cancellationToken: cancellationToken);
                return;
            }

            // Иначе пытаемся распарсить время
            if (!TimeSpan.TryParseExact(args[1], @"hh\:mm", CultureInfo.InvariantCulture, out var remindTime))
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Неверный формат. Используй: `/remind 23:00` или `/remind off`",
                    cancellationToken: cancellationToken);
                return;
            }

            await _reminderService.SetReminderAsync(message.From!.Id, remindTime, cancellationToken);

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Окей! Буду напоминать тебе ложиться спать в {remindTime:hh\\:mm} 💤",
                cancellationToken: cancellationToken);
        }
    }
}
