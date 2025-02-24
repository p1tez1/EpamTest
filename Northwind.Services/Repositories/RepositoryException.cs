namespace Northwind.Services.Repositories;

/// <summary>
/// Represents an exception thrown by a repository method.
/// </summary>
public class RepositoryException : Exception
{
    public RepositoryException()
    {
    }

    public RepositoryException(string message)
        : base(message)
    {
    }

    public RepositoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
