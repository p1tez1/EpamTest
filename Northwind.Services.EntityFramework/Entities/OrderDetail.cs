namespace Northwind.Services.EntityFramework.Entities;

public class OrderDetail
{
    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public double UnitPrice { get; set; }

    public long Quantity { get; set; }

    public double Discount { get; set; }

    public Order Order { get; set; }
    public Product Product { get; set; }
}
