using System.Diagnostics;

namespace Northwind.Services.Repositories;

[DebuggerDisplay("{Id}, {FirstName}, {LastName}")]
public class Employee
{
    public Employee(long id)
    {
        this.Id = id;
    }

    public long Id { get; private set; }

    public string FirstName { get; init; } = default!;

    public string LastName { get; init; } = default!;

    public string Country { get; init; } = default!;
}
