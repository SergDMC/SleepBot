using SleepBot.Core.Interfaces;
using SleepBot.Infrastructure.Data;

using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Npgsql;

namespace SleepBot.Handlers
{
    public class StartCommandHandler : ICommandHandler
    {
        public string Command => "start";

        private readonly ITelegramBotClient _botClient;
        private readonly DbConnectionFactory _connectionFactory;

        public StartCommandHandler(ITelegramBotClient botClient, DbConnectionFactory connectionFactory)
        {
            _botClient = botClient;
            _connectionFactory = connectionFactory;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;

            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Проверяем, есть ли уже пользователь
            var checkCmd = new NpgsqlCommand("SELECT COUNT(1) FROM Users WHERE ChatId = @chatId", connection);
            checkCmd.Parameters.AddWithValue("chatId", chatId);

            var exists = (long)await checkCmd.ExecuteScalarAsync(cancellationToken);

            if (exists == 0)
            {
                // Новый пользователь → добавляем в БД
                var insertCmd = new NpgsqlCommand(
                    "INSERT INTO Users (ChatId, CreatedAt, Username) VALUES (@chatId, @createdAt, @username) ON CONFLICT (ChatId) DO UPDATE SET Username = EXCLUDED.Username;",
                    connection
                );
                insertCmd.Parameters.AddWithValue("chatId", chatId);
                insertCmd.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
                insertCmd.Parameters.AddWithValue("username", (object?)message.Chat.Username ?? DBNull.Value);

                await insertCmd.ExecuteNonQueryAsync(cancellationToken);

                await _botClient.SendTextMessageAsync(
                    chatId,
                    "👋 Привет! Я *SleepBot* – твой помощник для контроля сна.\n" +
                    "Напиши /help, чтобы узнать, что я умею.",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                // Уже зарегистрирован → приветствуем снова
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "⚡ Ты уже зарегистрирован! Напиши /help, чтобы вспомнить команды.",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
