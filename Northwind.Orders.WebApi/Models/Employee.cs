namespace Northwind.Orders.WebApi.Models;

public class Employee
{
    public long Id { get; set; }

    public string FirstName { get; init; } = default!;

    public string LastName { get; init; } = default!;

    public string Country { get; init; } = default!;
}
