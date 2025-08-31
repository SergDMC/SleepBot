using System;

using Npgsql;

namespace SleepBot.Infrastructure.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        var db = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        _connectionString = $"Host={host};Database={db};Username={user};Password={pass};";
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
