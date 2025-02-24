using System.Diagnostics;

namespace Northwind.Services.Repositories;

[DebuggerDisplay("{Id}, {ProductName}")]
public class Product
{
    public Product(long id)
    {
        this.Id = id;
    }

    public long Id { get; private set; }

    public string ProductName { get; init; } = default!;

    public long SupplierId { get; init; }

    public string Supplier { get; init; } = default!;

    public long CategoryId { get; init; }

    public string Category { get; init; } = default!;
}
