namespace Northwind.Orders.WebApi.Models;

public class FullOrderDetail
{
    public long ProductId { get; set; }

    public string ProductName { get; set; } = default!;

    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = default!;

    public long SupplierId { get; set; }

    public string SupplierCompanyName { get; set; } = default!;

    public double UnitPrice { get; set; }

    public long Quantity { get; set; }

    public double Discount { get; set; }
}
