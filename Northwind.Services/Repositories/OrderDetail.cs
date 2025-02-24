using System.Diagnostics;

namespace Northwind.Services.Repositories;

[DebuggerDisplay("{Order.Id}, {Product.Id}")]
public class OrderDetail
{
    public OrderDetail(Order order)
    {
        this.Order = order;
    }

    public Order Order { get; private set; }

    public Product Product { get; init; } = default!;

    public double UnitPrice { get; init; }

    public long Quantity { get; init; }

    public double Discount { get; init; }
}
