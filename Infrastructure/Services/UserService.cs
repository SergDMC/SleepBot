using SleepBot.Core.Interfaces;
using SleepBot.Infrastructure.Data;
using System.Threading.Tasks;
using System.Threading;
using System;

public class UserService : IUserService
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserService(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task RegisterUserAsync(long chatId, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Проверим, есть ли пользователь
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE ChatId = @chatId";
        checkCmd.Parameters.AddWithValue("chatId", chatId);

        var count = (long)await checkCmd.ExecuteScalarAsync(cancellationToken);

        if (count == 0)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Users (ChatId, CreatedAt) VALUES (@chatId, @createdAt)";
            insertCmd.Parameters.AddWithValue("chatId", chatId);
            insertCmd.Parameters.AddWithValue("createdAt", DateTime.UtcNow);

            await insertCmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
