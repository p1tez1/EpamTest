namespace Northwind.Services.EntityFramework.Entities;

public class Product
{
    public long Id { get; set; }

    public string ProductName { get; set; }

    public long SupplierId { get; set; }

    public long CategoryId { get; set; }

    public string QuantityPerUnit { get; set; }

    public double UnitPrice { get; set; }

    public long UnitsInStock { get; set; }

    public long UnitsOnOrder { get; set; }

    public long ReorderLevel { get; set; }

    public long Discontinued { get; set; }

    public List<OrderDetail> OrderDetails {  get; set; }

    public Supplier Supplier { get; set; }

    public Category Category { get; set; }
}
