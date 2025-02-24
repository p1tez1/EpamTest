using System.Data.Common;

namespace Northwind.Services.EntityFramework.Tests;

public class SqlScriptException : DbException
{
    public SqlScriptException()
    {
    }

    public SqlScriptException(string message)
        : base(message)
    {
    }

    public SqlScriptException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
