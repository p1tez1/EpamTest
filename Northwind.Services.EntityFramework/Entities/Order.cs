namespace Northwind.Services.EntityFramework.Entities;

public class Order
{
    public long Id { get; set; }

    public string CustomerId { get; set; }

    public long ShipperId { get; set; }

    public long EmployeeId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public double Freight { get; set; }

    public string ShipName { get; set; }

    public string ShipAddress { get; set; }

    public string ShipCity { get; set; }

    public string? ShipRegion { get; set; }

    public string ShipPostalCode { get; set; }

    public string ShipCountry { get; set; }

    public Employee Employee { get; set; }

    public Shipper Shipper { get; set; }

    public List<OrderDetail> OrderDetails { get; set; }

    public Customer Customer { get; set; }
}
