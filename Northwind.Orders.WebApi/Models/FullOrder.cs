namespace Northwind.Orders.WebApi.Models;

public class FullOrder
{
    public long Id { get; set; }

    public Customer Customer { get; set; } = default!;

    public Employee Employee { get; set; } = default!;

    public DateTime OrderDate { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public Shipper Shipper { get; set; } = default!;

    public double Freight { get; set; }

    public string ShipName { get; set; } = default!;

    public ShippingAddress ShippingAddress { get; set; } = default!;

    public List<FullOrderDetail> OrderDetails { get; set; } = default!;
}
