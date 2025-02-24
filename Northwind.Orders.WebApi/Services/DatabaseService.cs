using Microsoft.Data.Sqlite;
using Northwind.Orders.WebApi.Properties;

namespace Northwind.Orders.WebApi.Services;

public sealed class DatabaseService : IDatabaseService, IDisposable
{
    private readonly SqliteConnection connection;

    public DatabaseService(string connectionString)
    {
        this.connection = new SqliteConnection(connectionString);
    }

    public void InitializeDatabase()
    {
        this.connection.Open();
        this.CreateTables();
        this.InitializeTables();
        this.InitializeOrders();
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

    private void CreateTables()
    {
        using var reader = new StringReader(Resources.CreateTables);
        ExecuteScript(this.connection, reader);
    }

    private void InitializeTables()
    {
        using var reader = new StringReader(Resources.InitializeTables);
        ExecuteScript(this.connection, reader);
    }

    private void InitializeOrders()
    {
        using var reader = new StringReader(Resources.InitializeOrders);
        ExecuteScript(this.connection, reader);
    }
}
