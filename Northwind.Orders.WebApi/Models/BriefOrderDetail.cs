namespace Northwind.Orders.WebApi.Models;

public class BriefOrderDetail
{
    public long ProductId { get; set; }

    public double UnitPrice { get; init; }

    public long Quantity { get; init; }

    public double Discount { get; init; }
}
