namespace Northwind.Orders.WebApi.Models;

public class ShippingAddress
{
    public string Address { get; set; } = default!;

    public string City { get; set; } = default!;

    public string? Region { get; set; }

    public string PostalCode { get; set; } = default!;

    public string Country { get; set; } = default!;
}
