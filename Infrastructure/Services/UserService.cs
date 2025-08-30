using SleepBot.Core.Interfaces;
using SleepBot.Infrastructure.Data;
using SleepBot.Core.Entities;
using System.Threading.Tasks;
using System.Threading;
using Dapper;
using Npgsql;
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
    public async Task SetTargetAsync(long chatId, int targetHours, CancellationToken cancellationToken)
    {
        const string sql = @"UPDATE Users 
                         SET SleepTargetHours = @targetHours 
                         WHERE ChatId = @chatId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(sql, new { chatId, targetHours });
    }

    public async Task<User?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
        SELECT ChatId, Username, CreatedAt, SleepTargetHours
        FROM Users
        WHERE ChatId = @chatId;
    ";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("chatId", chatId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new User
            {
                ChatId = reader.GetInt64(reader.GetOrdinal("ChatId")),
                Username = reader.IsDBNull(reader.GetOrdinal("Username"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Username")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                TargetHours = reader.IsDBNull(reader.GetOrdinal("SleepTargetHours"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("SleepTargetHours"))
            };
        }

        return null;
    }


}

