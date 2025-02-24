namespace Northwind.Services.EntityFramework.Entities;

public class Shipper
{
    public long Id { get; set; }

    public string CompanyName { get; set; }

    public string Phone { get; set; }

    public List<Order> Orders { get; set; }
}
