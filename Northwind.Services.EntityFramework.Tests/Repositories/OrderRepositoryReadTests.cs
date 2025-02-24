using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.EntityFramework.Repositories;
using Northwind.Services.Repositories;
using NUnit.Framework;
using RepositoryCustomer = Northwind.Services.Repositories.Customer;
using RepositoryCustomerCode = Northwind.Services.Repositories.CustomerCode;
using RepositoryEmployee = Northwind.Services.Repositories.Employee;
using RepositoryOrder = Northwind.Services.Repositories.Order;
using RepositoryOrderDetail = Northwind.Services.Repositories.OrderDetail;
using RepositoryProduct = Northwind.Services.Repositories.Product;
using RepositoryShipper = Northwind.Services.Repositories.Shipper;

namespace Northwind.Services.EntityFramework.Tests.Repositories;

[TestFixture]
public sealed class OrderRepositoryReadTests : IDisposable
{
    private static readonly object[] GetOrdersAsync =
    {
        new object[] { 0, 1, new long[] { 10248 } },
        new object[] { 0, 10, new long[] { 10248, 10249, 10250, 10251, 10252, 10253, 10254, 10255, 10256, 10257 } },
        new object[] { 0, 20, new long[] { 10248, 10249, 10250, 10251, 10252, 10253, 10254, 10255, 10256, 10257, 10258, 10259, 10260, 10261, 10262, 10263, 10264, 10265, 10266, 10267 } },
        new object[] { 10, 1, new long[] { 10258 } },
        new object[] { 10, 10, new long[] { 10258, 10259, 10260, 10261, 10262, 10263, 10264, 10265, 10266, 10267 } },
        new object[] { 10, 20, new long[] { 10258, 10259, 10260, 10261, 10262, 10263, 10264, 10265, 10266, 10267, 10268, 10269, 10270, 10271, 10272, 10273, 10274, 10275, 10276, 10277 } },
    };

    private DatabaseService databaseService = default!;
    private OrderRepository repository = default!;

    [TestCase(-1, 1)]
    [TestCase(0, -1)]
    [TestCase(0, 0)]
    public void GetOrdersAsync_ArgumentsAreOutOfRange_ThrowsArgumentOutOfRangeException(int skip, int count)
    {
        _ = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await this.repository.GetOrdersAsync(skip: skip, count: count));
    }

    [TestCaseSource(nameof(GetOrdersAsync))]
    public async Task GetOrdersAsync_ArgumentsAreValid_ReturnsOrderList(int skip, int count, long[] id)
    {
        ArgumentNullException.ThrowIfNull(id);

        // Act
        IList<RepositoryOrder> orders = await this.repository.GetOrdersAsync(skip: skip, count: count);

        // Assert
        Assert.That(orders.Count, Is.EqualTo(id.Length));

        for (int i = 0; i < orders.Count; i++)
        {
            Assert.That(orders[i].Id, Is.EqualTo(id[i]));
        }
    }

    [Test]
    public void GetOrderAsync_OrderIsNotExist_ThrowsRepositoryException()
    {
        _ = Assert.ThrowsAsync<OrderNotFoundException>(async () => await this.repository.GetOrderAsync(orderId: 0));
    }

    [TestCaseSource(nameof(GetOrderDataAsync))]
    public async Task GetOrderAsync_OrderIsExist_ReturnsOrder(RepositoryOrder expectedOrder)
    {
        ArgumentNullException.ThrowIfNull(expectedOrder);

        // Act
        RepositoryOrder? actualOrder = await this.repository.GetOrderAsync(orderId: expectedOrder.Id);

        // Assert
        Assert.That(actualOrder, Is.Not.Null);
        var compareResult = OrderComparer.Compare(actualOrder!, expectedOrder, out string message);
        Assert.That(compareResult, Is.True, message);
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        this.databaseService = new DatabaseService();
        this.databaseService.CreateTables();
        this.databaseService.InitializeTables();
        this.databaseService.InitializeOrders();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        this.repository = new OrderRepository(context);
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        this.databaseService.Dispose();
    }

    private static IEnumerable<RepositoryOrder> GetOrderDataAsync()
    {
        RepositoryOrder order = new RepositoryOrder(10248)
        {
            Customer = new RepositoryCustomer(new RepositoryCustomerCode("VINET"))
            {
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new RepositoryEmployee(5)
            {
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            Shipper = new RepositoryShipper(3)
            {
                CompanyName = "Federal Shipping",
            },
            ShippingAddress = new ShippingAddress("59 rue de l'Abbaye", "Reims", null, "51100", "France"),
            OrderDate = new DateTime(1996, 7, 4),
            RequiredDate = new DateTime(1996, 8, 1),
            ShippedDate = new DateTime(1996, 7, 16),
            Freight = 32.38,
            ShipName = "Vins et alcools Chevalier",
        };

        order.OrderDetails.Add(new RepositoryOrderDetail(order)
        {
            Product = new RepositoryProduct(11)
            {
                ProductName = "Queso Cabrales",
                CategoryId = 4,
                Category = "Dairy Products",
                SupplierId = 5,
                Supplier = "Cooperativa de Quesos 'Las Cabras'",
            },
            UnitPrice = 14,
            Quantity = 12,
            Discount = 0,
        });

        order.OrderDetails.Add(new RepositoryOrderDetail(order)
        {
            Product = new RepositoryProduct(42)
            {
                ProductName = "Singaporean Hokkien Fried Mee",
                CategoryId = 5,
                Category = "Grains/Cereals",
                SupplierId = 20,
                Supplier = "Leka Trading",
            },
            UnitPrice = 9.8,
            Quantity = 10,
            Discount = 0,
        });

        order.OrderDetails.Add(new RepositoryOrderDetail(order)
        {
            Product = new RepositoryProduct(72)
            {
                ProductName = "Mozzarella di Giovanni",
                CategoryId = 4,
                Category = "Dairy Products",
                SupplierId = 14,
                Supplier = "Formaggi Fortini s.r.l.",
            },
            UnitPrice = 34.8,
            Quantity = 5,
            Discount = 0,
        });

        yield return order;
    }
}
