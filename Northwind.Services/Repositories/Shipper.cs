using System.Diagnostics;

namespace Northwind.Services.Repositories;

[DebuggerDisplay("{Id}, {CompanyName}")]
public class Shipper
{
    public Shipper(long id)
    {
        this.Id = id;
    }

    public long Id { get; private set; }

    public string CompanyName { get; init; } = default!;
}
