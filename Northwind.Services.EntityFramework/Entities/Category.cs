namespace Northwind.Services.EntityFramework.Entities;

public class Category
{
    public long Id { get; set; }

    public string CategoryName { get; set; }

    public string Description { get; set; }

    public List<Product> Products { get; set; }
}
