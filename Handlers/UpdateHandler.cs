using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using SleepBot.Core.Interfaces;

namespace SleepBot.Handlers
{
    public class UpdateHandler
    {
        private readonly IServiceProvider _services;

        public UpdateHandler(IServiceProvider services)
        {
            _services = services;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message || update.Message is null || update.Message.Text is null)
                return;

            var message = update.Message;
            var commandText = message.Text.Split(' ')[0].ToLowerInvariant();

            using var scope = _services.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<ICommandHandler>();

            var handler = handlers.FirstOrDefault(h => $"/{h.Command}" == commandText);
            if (handler is not null)
            {
                await handler.HandleAsync(message, cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Неизвестная команда. Доступные команды: /sleep, /stats, /remind",
                    cancellationToken: cancellationToken);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiEx => $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
