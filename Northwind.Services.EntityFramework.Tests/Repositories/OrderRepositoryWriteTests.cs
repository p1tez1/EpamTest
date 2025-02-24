using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Repositories;
using Northwind.Services.Repositories;
using NUnit.Framework;
using NorthwindContext = Northwind.Services.EntityFramework.Entities.NorthwindContext;

namespace Northwind.Services.EntityFramework.Tests.Repositories;

[TestFixture]
public class OrderRepositoryWriteTests
{
    [TestCase(10248, 3)]
    [TestCase(11077, 25)]
    public async Task RemoveOrderAsync_OrderIsExist_RemovesOrder(long orderId, int detailCount)
    {
        // Arrange
        using var databaseService = new DatabaseService();
        databaseService.CreateTables();
        databaseService.InitializeTables();
        databaseService.InitializeOrders();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        var repository = new OrderRepository(context);

        // Preconditions
        long ordersBefore = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders WHERE OrderID = {orderId}");
        Assert.That(ordersBefore, Is.EqualTo(1));

        long orderDetailsBefore = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM OrderDetails WHERE OrderID = {orderId}");
        Assert.That(orderDetailsBefore, Is.EqualTo(detailCount));

        // Act
        await repository.RemoveOrderAsync(orderId: orderId);

        // Postconditions
        long ordersAfter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders WHERE OrderID = {orderId}");
        Assert.That(ordersAfter, Is.EqualTo(0));

        long orderDetailsAfter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM OrderDetails WHERE OrderID = {orderId}");
        await context.DisposeAsync();
        Assert.That(orderDetailsAfter, Is.EqualTo(0));
    }

    [TestCase(10247)]
    [TestCase(11078)]
    public void RemoveOrderAsync_OrderIsNotExist_ThrowsOrderNotFoundException(long orderId)
    {
        // Arrange
        using var databaseService = new DatabaseService();
        databaseService.CreateTables();
        databaseService.InitializeTables();
        databaseService.InitializeOrders();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        var repository = new OrderRepository(context);

        // Preconditions
        long countBefore = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders Where OrderID = {orderId}");
        Assert.That(countBefore, Is.EqualTo(0));

        // Act
        _ = Assert.ThrowsAsync<OrderNotFoundException>(async () => await repository.RemoveOrderAsync(orderId: orderId));

        // Postconditions
        long countAfter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders Where OrderID = {orderId}");
        context.Dispose();
        Assert.That(countAfter, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(AddOrderData))]
    public async Task AddOrderAsync_TableIsEmpty_AddsOrderAndReturnsRowId(Order order)
    {
        // Arrange
        using var databaseService = new DatabaseService();
        databaseService.CreateTables();
        databaseService.InitializeTables();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        var repository = new OrderRepository(context);

        // Preconditions
        var ordersBefore = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM Orders");
        Assert.That(ordersBefore, Is.EqualTo(0));

        var orderDetailsBefore = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM OrderDetails");
        Assert.That(orderDetailsBefore, Is.EqualTo(0));

        // Act
        long rowId = await repository.AddOrderAsync(order);

        // Assert
        Assert.That(rowId, Is.GreaterThan(0));

        // Postconditions
        var ordersAfter = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM Orders");
        Assert.That(ordersAfter, Is.EqualTo(1));

        var orderDetailsAfter = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM OrderDetails");
        await context.DisposeAsync();
        ArgumentNullException.ThrowIfNull(order);
        Assert.That(orderDetailsAfter, Is.EqualTo(order.OrderDetails.Count));
    }

    [TestCaseSource(nameof(AddOrderData))]
    public async Task AddOrderAsync_TableIsNotEmpty_AddsOrderAndReturnsRowId(Order order)
    {
        // Arrange
        using var databaseService = new DatabaseService();
        databaseService.CreateTables();
        databaseService.InitializeTables();
        databaseService.InitializeOrders();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        var repository = new OrderRepository(context);

        // Act
        long rowId = await repository.AddOrderAsync(order);

        // Assert
        long counter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders WHERE OrderID = {rowId}");
        Assert.That(counter, Is.EqualTo(1));

        counter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM OrderDetails WHERE OrderID = {rowId}");
        await context.DisposeAsync();
        ArgumentNullException.ThrowIfNull(order);
        Assert.That(counter, Is.EqualTo(order.OrderDetails.Count));
    }

    [TestCaseSource(nameof(AddOrderDataInvalidOrderDetails))]
    public void AddOrderAsync_InvalidOrderDetailData_ThrowsRepositoryException(Order order)
    {
        // Arrange
        using var databaseService = new DatabaseService();
        databaseService.CreateTables();
        databaseService.InitializeTables();
        databaseService.InitializeOrders();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        var repository = new OrderRepository(context);

        var ordersBefore = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM Orders");
        var orderDetailsBefore = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM OrderDetails");

        // Act
        _ = Assert.ThrowsAsync<RepositoryException>(async () => _ = await repository.AddOrderAsync(order));

        // Postconditions
        var ordersAfter = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM Orders");
        Assert.That(ordersAfter, Is.EqualTo(ordersBefore));

        var orderDetailsAfter = databaseService.ExecuteScalar<long>("SELECT COUNT(*) FROM OrderDetails");
        context.Dispose();
        Assert.That(orderDetailsAfter, Is.EqualTo(orderDetailsBefore));
    }

    [TestCaseSource(nameof(UpdateOrderAsyncData))]
    public async Task UpdateOrderAsync_OrderIsExist_UpdatesOrder(Order order)
    {
        // Arrange
        using var databaseService = new DatabaseService();
        databaseService.CreateTables();
        databaseService.InitializeTables();
        databaseService.InitializeOrders();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        var repository = new OrderRepository(context);
        ArgumentNullException.ThrowIfNull(order);

        // Preconditions
        long ordersBefore = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders WHERE OrderID = {order.Id}");
        Assert.That(ordersBefore, Is.EqualTo(1));

        long orderDetailsBefore = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM OrderDetails WHERE OrderID = {order.Id}");
        Assert.That(orderDetailsBefore, Is.GreaterThan(0));

        // Act
        await repository.UpdateOrderAsync(order: order);

        // Postconditions
        long ordersAfter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders WHERE OrderID = {order.Id}");
        Assert.That(ordersAfter, Is.EqualTo(1));

        long orderDetailsAfter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM OrderDetails WHERE OrderID = {order.Id}");
        Assert.That(orderDetailsAfter, Is.EqualTo(order.OrderDetails.Count));

        // Assert
        Order updatedOrder = await repository.GetOrderAsync(order.Id);
        var result = OrderComparer.Compare(updatedOrder, order, out string message);
        await context.DisposeAsync();
        Assert.That(result, Is.True, message);
    }

    [TestCaseSource(nameof(UpdateOrderAsyncOrderIsNotExistData))]
    public void UpdateOrderAsync_OrderIsNotExist_OrderNotFoundException(Order order)
    {
        // Arrange
        using var databaseService = new DatabaseService();
        databaseService.CreateTables();
        databaseService.InitializeTables();
        databaseService.InitializeOrders();

        var contextOptions = SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), DatabaseService.ConnectionString);
        var context = new NorthwindContext(contextOptions.Options);
        var repository = new OrderRepository(context);
        ArgumentNullException.ThrowIfNull(order);

        // Preconditions
        long ordersBefore = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders WHERE OrderID = {order.Id}");
        Assert.That(ordersBefore, Is.EqualTo(0));

        long orderDetailsBefore = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM OrderDetails WHERE OrderID = {order.Id}");
        Assert.That(orderDetailsBefore, Is.EqualTo(0));

        // Act
        _ = Assert.ThrowsAsync<OrderNotFoundException>(async () => await repository.UpdateOrderAsync(order: order));

        // Postconditions
        long ordersAfter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM Orders WHERE OrderID = {order.Id}");
        Assert.That(ordersAfter, Is.EqualTo(0));

        long orderDetailsAfter = databaseService.ExecuteScalar<long>($"SELECT COUNT(*) FROM OrderDetails WHERE OrderID = {order.Id}");
        context.Dispose();
        Assert.That(orderDetailsAfter, Is.EqualTo(0));
    }

    private static IEnumerable<Order> AddOrderData()
    {
        Order order = new Order(10248)
        {
            Customer = new Customer(new CustomerCode("VINET"))
            {
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new Employee(5)
            {
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            Shipper = new Shipper(3)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(11)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(42)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(72)
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

    private static IEnumerable<Order> AddOrderDataInvalidOrderDetails()
    {
        Order order = new Order(10248)
        {
            Customer = new Customer(new CustomerCode("VINET"))
            {
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new Employee(5)
            {
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            Shipper = new Shipper(3)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(0) // Invalid ProductID
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

        yield return order;

        order = new Order(10248)
        {
            Customer = new Customer(new CustomerCode("VINET"))
            {
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new Employee(5)
            {
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            Shipper = new Shipper(3)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(11)
            {
                ProductName = "Queso Cabrales",
                CategoryId = 4,
                Category = "Dairy Products",
                SupplierId = 5,
                Supplier = "Cooperativa de Quesos 'Las Cabras'",
            },
            UnitPrice = -1, // Invalid UnitPrice
            Quantity = 12,
            Discount = 0,
        });

        yield return order;

        order = new Order(10248)
        {
            Customer = new Customer(new CustomerCode("VINET"))
            {
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new Employee(5)
            {
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            Shipper = new Shipper(3)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(11)
            {
                ProductName = "Queso Cabrales",
                CategoryId = 4,
                Category = "Dairy Products",
                SupplierId = 5,
                Supplier = "Cooperativa de Quesos 'Las Cabras'",
            },
            UnitPrice = 14,
            Quantity = 0, // Invalid Quantity
            Discount = 0,
        });

        yield return order;

        order = new Order(10248)
        {
            Customer = new Customer(new CustomerCode("VINET"))
            {
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new Employee(5)
            {
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            Shipper = new Shipper(3)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(11)
            {
                ProductName = "Queso Cabrales",
                CategoryId = 4,
                Category = "Dairy Products",
                SupplierId = 5,
                Supplier = "Cooperativa de Quesos 'Las Cabras'",
            },
            UnitPrice = 14,
            Quantity = 12,
            Discount = -1, // Invalid Discount
        });

        yield return order;
    }

    private static IEnumerable<Order> UpdateOrderAsyncData()
    {
        Order order = new Order(10248)
        {
            Customer = new Customer(new CustomerCode("ALFKI"))
            {
                CompanyName = "Alfreds Futterkiste",
            },
            Employee = new Employee(1)
            {
                FirstName = "Nancy",
                LastName = "Davolio",
                Country = "USA",
            },
            Shipper = new Shipper(1)
            {
                CompanyName = "Speedy Express",
            },
            ShippingAddress = new ShippingAddress("address", "city", "region", "postal code", "country"),
            OrderDate = new DateTime(1990, 1, 2),
            RequiredDate = new DateTime(1991, 3, 4),
            ShippedDate = new DateTime(1993, 5, 6),
            Freight = 123_456.12,
            ShipName = "Alfreds Futterkiste",
        };

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(1)
            {
                ProductName = "Chai",
                CategoryId = 1,
                Category = "Beverages",
                SupplierId = 1,
                Supplier = "Exotic Liquids",
            },
            UnitPrice = 123_456,
            Quantity = 123_456,
            Discount = 1,
        });

        yield return order;
    }

    private static IEnumerable<Order> UpdateOrderAsyncOrderIsNotExistData()
    {
        Order order = new Order(1)
        {
            Customer = new Customer(new CustomerCode("VINET"))
            {
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new Employee(5)
            {
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            Shipper = new Shipper(3)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(11)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(42)
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

        order.OrderDetails.Add(new OrderDetail(order)
        {
            Product = new Product(72)
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
