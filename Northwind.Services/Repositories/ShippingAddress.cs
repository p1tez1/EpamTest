using System.Diagnostics;

namespace Northwind.Services.Repositories;

[DebuggerDisplay("{Country}, {City}, {Address}")]
public class ShippingAddress
{
    public ShippingAddress(string address, string city, string? region, string postalCode, string country)
    {
        this.Address = !string.IsNullOrWhiteSpace(address) ? address : throw new ArgumentException("address argument is null, empty, or consists only of white-space characters.", nameof(address));
        this.City = !string.IsNullOrWhiteSpace(city) ? city : throw new ArgumentException("city argument is null, empty, or consists only of white-space characters.", nameof(city));
        this.Region = region;
        this.PostalCode = !string.IsNullOrWhiteSpace(postalCode) ? postalCode : throw new ArgumentException("postalCode argument is null, empty, or consists only of white-space characters.", nameof(postalCode));
        this.Country = !string.IsNullOrWhiteSpace(country) ? country : throw new ArgumentException("country argument is null, empty, or consists only of white-space characters.", nameof(country));
    }

    public string Address { get; private set; }

    public string City { get; private set; }

    public string? Region { get; private set; }

    public string PostalCode { get; private set; }

    public string Country { get; private set; }
}
