using System.Diagnostics;

namespace Northwind.Services.Repositories;

[DebuggerDisplay("{Code}, {CompanyName}")]
public class Customer
{
    public Customer(CustomerCode code)
    {
        this.Code = code;
    }

    public CustomerCode Code { get; private set; }

    public string CompanyName { get; init; } = default!;
}
