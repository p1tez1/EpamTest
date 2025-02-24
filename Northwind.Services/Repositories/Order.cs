using System.Diagnostics;

namespace Northwind.Services.Repositories;

/// <summary>
/// Represents an order.
/// </summary>
[DebuggerDisplay("Order #{Id}")]
public class Order
{
    public Order(long id)
    {
        this.Id = id;
    }

    public long Id { get; private set; }

    public Customer Customer { get; set; } = default!;

    public Employee Employee { get; set; } = default!;

    public DateTime OrderDate { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public Shipper Shipper { get; set; } = default!;

    public double Freight { get; set; }

    public string ShipName { get; set; } = default!;

    public ShippingAddress ShippingAddress { get; set; } = default!;

    public IList<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
