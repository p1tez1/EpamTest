using Northwind.Services.Repositories;

namespace Northwind.Services.EntityFramework.Tests.Repositories;

public static class OrderComparer
{
    public static bool Compare(Order o1, Order o2, out string message)
    {
        message = string.Empty;

        if (ReferenceEquals(o1, o2))
        {
            return true;
        }

        ArgumentNullException.ThrowIfNull(o1);
        ArgumentNullException.ThrowIfNull(o2);

        if (o1.Id != o2.Id)
        {
            message = $"{o1.Id} != {o2.Id}";
            return false;
        }

        if (!Equals(o1.Customer, o2.Customer, out var innerMessage))
        {
            message = innerMessage;
            return false;
        }

        if (!Equals(o1.Employee, o2.Employee, out innerMessage))
        {
            message = innerMessage;
            return false;
        }

        if (o1.OrderDate != o2.OrderDate)
        {
            message = "OrderDate != OrderDate";
            return false;
        }

        if (o1.RequiredDate != o2.RequiredDate)
        {
            message = "RequiredDate != RequiredDate";
            return false;
        }

        if (o1.ShippedDate != o2.ShippedDate)
        {
            message = "ShippeddDate != ShippeddDate";
            return false;
        }

        if (!Equals(o1.Shipper, o2.Shipper, out innerMessage))
        {
            message = innerMessage;
            return false;
        }

        if (o1.Freight != o2.Freight)
        {
            message = "Freight != Freight";
            return false;
        }

        if (!string.Equals(o1.ShipName, o2.ShipName, StringComparison.Ordinal))
        {
            message = "ShipName != ShipName";
            return false;
        }

        if (!Equals(o1.ShippingAddress, o2.ShippingAddress, out innerMessage))
        {
            message = innerMessage;
            return false;
        }

        if (o1.OrderDetails.Count != o2.OrderDetails.Count)
        {
            message = "OrderDetails.Count != OrderDetails.Count";
            return false;
        }

        for (int i = 0; i < o1.OrderDetails.Count; i++)
        {
            if (!Equals(o1.OrderDetails[i], o2.OrderDetails[i], out innerMessage))
            {
                message = innerMessage;
                return false;
            }
        }

        return true;
    }

    private static bool Equals(Customer c1, Customer c2, out string message)
    {
        message = string.Empty;

        if (!Equals(c1.Code, c2.Code, out string innerMessage))
        {
            message = innerMessage;
            return false;
        }

        if (!string.Equals(c1.CompanyName, c2.CompanyName, StringComparison.Ordinal))
        {
            message = "Customer CompanyName != Customer CompanyName";
            return false;
        }

        return true;
    }

    private static bool Equals(CustomerCode c1, CustomerCode c2, out string message)
    {
        message = string.Empty;

        if (!string.Equals(c1.Code, c2.Code, StringComparison.Ordinal))
        {
            message = "Customer Code != Customer Code";
            return false;
        }

        return true;
    }

    private static bool Equals(Employee e1, Employee e2, out string message)
    {
        message = string.Empty;

        if (e1.Id != e2.Id)
        {
            message = "EmployeeID != EmployeeID";
            return false;
        }

        if (!string.Equals(e1.FirstName, e2.FirstName, StringComparison.Ordinal))
        {
            message = "Employee First Name != Employee First Name";
            return false;
        }

        if (!string.Equals(e1.LastName, e2.LastName, StringComparison.Ordinal))
        {
            message = "Employee Last Name != Employee Last Name";
            return false;
        }

        if (!string.Equals(e1.Country, e2.Country, StringComparison.Ordinal))
        {
            message = "Employee Country != Employee Country";
            return false;
        }

        return true;
    }

    private static bool Equals(ShippingAddress a1, ShippingAddress a2, out string message)
    {
        message = string.Empty;

        if (!string.Equals(a1.Address, a2.Address, StringComparison.Ordinal))
        {
            message = "ShippingAddress Address != ShippingAddress Address";
            return false;
        }

        if (!string.Equals(a1.City, a2.City, StringComparison.Ordinal))
        {
            message = "ShippingAddress City != ShippingAddress City";
            return false;
        }

        if (!string.Equals(a1.Region, a2.Region, StringComparison.Ordinal))
        {
            message = "ShippingAddress Region != ShippingAddress Region";
            return false;
        }

        if (!string.Equals(a1.PostalCode, a2.PostalCode, StringComparison.Ordinal))
        {
            message = "ShippingAddress PostalCode != ShippingAddress PostalCode";
            return false;
        }

        if (!string.Equals(a1.Country, a2.Country, StringComparison.Ordinal))
        {
            message = "ShippingAddress Country != ShippingAddress Country";
            return false;
        }

        return true;
    }

    private static bool Equals(Shipper s1, Shipper s2, out string message)
    {
        message = string.Empty;

        if (s1.Id != s2.Id)
        {
            message = "Shipper Id != Shipper Id";
            return false;
        }

        if (!string.Equals(s1.CompanyName, s2.CompanyName, StringComparison.Ordinal))
        {
            message = "Shipper CompanyName != Shipper CompanyName";
            return false;
        }

        return true;
    }

    private static bool Equals(OrderDetail d1, OrderDetail d2, out string message)
    {
        message = string.Empty;

        if (!Equals(d1.Product, d2.Product, out string innerMessage))
        {
            message = innerMessage;
            return false;
        }

        if (d1.UnitPrice != d2.UnitPrice)
        {
            message = "Product.UnitPrice != Product.UnitPrice";
            return false;
        }

        if (d1.Quantity != d2.Quantity)
        {
            message = "Product.Quantity != Product.Quantity";
            return false;
        }

        if (d1.Discount != d2.Discount)
        {
            message = "Product.Discount != Product.Discount";
            return false;
        }

        return true;
    }

    private static bool Equals(Product p1, Product p2, out string message)
    {
        message = string.Empty;

        if (p1.Id != p2.Id)
        {
            message = "Product.Id != Product.Id";
            return false;
        }

        if (!string.Equals(p1.ProductName, p2.ProductName, StringComparison.Ordinal))
        {
            message = "Product.ProductName != Product.ProductName";
            return false;
        }

        if (p1.SupplierId != p2.SupplierId)
        {
            message = "Product.SupplierId != Product.SupplierId";
            return false;
        }

        if (!string.Equals(p1.Supplier, p2.Supplier, StringComparison.Ordinal))
        {
            message = "Product.Supplier != Product.Supplier";
            return false;
        }

        if (p1.CategoryId != p2.CategoryId)
        {
            message = "Product.CategoryId != Product.CategoryId";
            return false;
        }

        if (!string.Equals(p1.Category, p2.Category, StringComparison.Ordinal))
        {
            message = "Product.Category != Product.Category";
            return false;
        }

        return true;
    }
}
