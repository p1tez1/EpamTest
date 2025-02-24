namespace Northwind.Orders.WebApi.Models;

public class BriefOrder
{
    public long Id { get; set; }

    public string CustomerId { get; set; } = default!;

    public long EmployeeId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public long ShipperId { get; set; }

    public double Freight { get; set; }

    public string ShipName { get; set; } = default!;

    public string ShipAddress { get; set; } = default!;

    public string ShipCity { get; set; } = default!;

    public string? ShipRegion { get; set; }

    public string ShipPostalCode { get; set; } = default!;

    public string ShipCountry { get; set; } = default!;

    public List<BriefOrderDetail> OrderDetails { get; set; } = default!;
}
