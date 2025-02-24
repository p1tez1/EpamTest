using Microsoft.Data.Sqlite;

namespace Northwind.Services.EntityFramework.Tests;

public sealed class DatabaseService : IDisposable
{
    public const string ConnectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

    private readonly SqliteConnection connection;

    public DatabaseService()
    {
        this.connection = new SqliteConnection(ConnectionString);
        this.connection.Open();
    }

    public void CreateTables()
    {
        using var reader = new StringReader(Properties.Resources.CreateTables);
        ExecuteScript(this.connection, reader);
    }

    public void InitializeTables()
    {
        using var reader = new StringReader(Properties.Resources.InitializeTables);
        ExecuteScript(this.connection, reader);
    }

    public void InitializeOrders()
    {
        using var reader = new StringReader(Properties.Resources.InitializeOrders);
        ExecuteScript(this.connection, reader);
    }

    public T ExecuteScalar<T>(string commandText)
    {
        using var command = this.connection.CreateCommand();
        command.CommandText = commandText;
        object? result = command.ExecuteScalar();
        return result is not null ? (T)result : throw new SqlScriptException($"SQL statement failed: {command}.");
    }

    public int ExecuteNonQuery(string commandText)
    {
        using var command = this.connection.CreateCommand();
        command.CommandText = commandText;
        return command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        this.connection.Dispose();
    }

    private static void ExecuteScript(SqliteConnection connection, TextReader reader)
    {
        using var transaction = connection.BeginTransaction();

        try
        {
            int lineNumber = 1;
            string? line;

            while ((line = reader.ReadLine()) is not null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    using var command = new SqliteCommand(line, connection, transaction);
                    _ = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SqlScriptException($"Exception during executing an SQL command on line {lineNumber}: \"{line}\".", e);
                }
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}
