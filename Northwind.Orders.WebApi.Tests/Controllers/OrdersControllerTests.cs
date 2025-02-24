using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Northwind.Orders.WebApi.Controllers;
using Northwind.Services.EntityFramework.Tests;
using Northwind.Services.Repositories;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ModelsAddOrder = Northwind.Orders.WebApi.Models.AddOrder;
using ModelsBriefOrder = Northwind.Orders.WebApi.Models.BriefOrder;
using ModelsBriefOrderDetail = Northwind.Orders.WebApi.Models.BriefOrderDetail;
using ModelsCustomer = Northwind.Orders.WebApi.Models.Customer;
using ModelsEmployee = Northwind.Orders.WebApi.Models.Employee;
using ModelsFullOrder = Northwind.Orders.WebApi.Models.FullOrder;
using ModelsShipper = Northwind.Orders.WebApi.Models.Shipper;
using ModelsShippingAddress = Northwind.Orders.WebApi.Models.ShippingAddress;
using RepositoryCustomer = Northwind.Services.Repositories.Customer;
using RepositoryCustomerCode = Northwind.Services.Repositories.CustomerCode;
using RepositoryEmployee = Northwind.Services.Repositories.Employee;
using RepositoryOrder = Northwind.Services.Repositories.Order;
using RepositoryOrderDetail = Northwind.Services.Repositories.OrderDetail;
using RepositoryProduct = Northwind.Services.Repositories.Product;
using RepositoryShipper = Northwind.Services.Repositories.Shipper;

namespace Northwind.Orders.WebApi.Tests.Controllers;

[TestFixture]
public sealed class OrdersControllerTests
{
    private static readonly object[][] ConstructorData =
    {
        new Type[] { typeof(IOrderRepository), typeof(ILogger<OrdersController>) },
    };

    [TestCaseSource(nameof(GetOrderAsyncData))]
    public async Task GetOrderAsync_OrderIsExist_ReturnsOrder(RepositoryOrder repositoryOrder, ModelsFullOrder expectedOrder)
    {
        ArgumentNullException.ThrowIfNull(repositoryOrder);
        ArgumentNullException.ThrowIfNull(expectedOrder);

        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.GetOrderAsync(It.Is<long>(arg => arg == expectedOrder.Id)))
            .ReturnsAsync(repositoryOrder)
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<ModelsFullOrder> actualResult = await controller.GetOrderAsync(orderId: expectedOrder.Id);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var actualResultValue = ((OkObjectResult)actualResult.Result!).Value;
        Assert.That(actualResultValue, Is.Not.Null);

        var comparer = new ObjectComparer();
        comparer.Compare(expectedOrder, actualResultValue);

        orderRepositoryMock.VerifyAll();
    }

    [TestCase(1)]
    public async Task GetOrderAsync_OrderIsNotExist_ReturnsNotFound(long orderId)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.GetOrderAsync(It.Is<long>((arg) => arg == orderId)))
            .ThrowsAsync(new OrderNotFoundException())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        var actualResult = await controller.GetOrderAsync(orderId: orderId);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Value, Is.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(NotFoundResult)));

        orderRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetOrderAsync_ExceptionThrown_ReturnsServerError()
    {
        long orderId = 1;

        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.GetOrderAsync(It.Is<long>((arg) => arg == orderId)))
            .ThrowsAsync(new Exception())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<ModelsFullOrder> actualResult = await controller.GetOrderAsync(orderId: orderId);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Value, Is.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(StatusCodeResult)));
        Assert.That(((StatusCodeResult)actualResult.Result!).StatusCode, Is.EqualTo(500));

        orderRepositoryMock.VerifyAll();
    }

    [TestCaseSource(nameof(GetOrdersAsyncData))]
    public async Task GetOrdersAsync_OrdersAreExists_ReturnsList(IList<RepositoryOrder> orderList, int? skip, int? count, IList<ModelsBriefOrder> expectedOrderList)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.GetOrdersAsync(It.Is<int>(arg1 => arg1 == skip), It.Is<int>(arg2 => arg2 == count)))
            .ReturnsAsync(orderList)
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<IEnumerable<ModelsBriefOrder>> actualResult = await controller.GetOrdersAsync(skip: skip, count: count);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var actualResultValue = ((OkObjectResult)actualResult.Result!).Value;
        Assert.That(actualResultValue, Is.Not.Null);
        var actualEnumerable = (IEnumerable<ModelsBriefOrder>)actualResultValue!;
        var actualList = actualEnumerable.ToList();

        ArgumentNullException.ThrowIfNull(expectedOrderList);
        Assert.That(actualList.Count, Is.EqualTo(expectedOrderList.Count));

        var comparer = new ObjectComparer();

        for (int i = 0; i < actualList.Count; i++)
        {
            comparer.Compare(expectedOrderList[i], actualList[i]);
        }

        orderRepositoryMock.VerifyAll();
    }

    [TestCase(null, 1, 0, 1)]
    [TestCase(100, null, 100, 10)]
    [TestCase(null, null, 0, 10)]
    public async Task GetOrdersAsync_DefaultArguments_ReturnsList(int? skip, int? count, int expectedSkip, int expectedCount)
    {
        // Arrange
        var orderList = new List<RepositoryOrder>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.GetOrdersAsync(It.Is<int>(arg1 => arg1 == expectedSkip), It.Is<int>(arg2 => arg2 == expectedCount)))
            .ReturnsAsync(orderList)
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<IEnumerable<ModelsBriefOrder>> actualResult = await controller.GetOrdersAsync(skip: skip, count: count);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var actualResultValue = ((OkObjectResult)actualResult.Result!).Value;
        Assert.That(actualResultValue, Is.Not.Null);
        var actualEnumerable = (IEnumerable<ModelsBriefOrder>)actualResultValue!;
        var actualList = actualEnumerable.ToList();
        Assert.That(actualList.Count, Is.EqualTo(0));
        orderRepositoryMock.VerifyAll();
    }

    [TestCase(-1, 1)]
    [TestCase(0, 0)]
    [TestCase(0, -1)]
    public async Task GetOrdersAsync_InvalidArguments_ReturnsBadRequest(int skip, int count)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<IEnumerable<ModelsBriefOrder>> actualResult = await controller.GetOrdersAsync(skip: skip, count: count);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(BadRequestResult)));
    }

    [Test]
    public async Task GetOrdersAsync_ExceptionThrown_ReturnsServerError()
    {
        var skip = 1;
        var count = 1;

        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.GetOrdersAsync(It.Is<int>(arg1 => arg1 == skip), It.Is<int>(arg2 => arg2 == count)))
            .ThrowsAsync(new Exception())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<IEnumerable<ModelsBriefOrder>> actualResult = await controller.GetOrdersAsync(skip: skip, count: count);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Value, Is.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(StatusCodeResult)));
        Assert.That(((StatusCodeResult)actualResult.Result!).StatusCode, Is.EqualTo(500));

        orderRepositoryMock.VerifyAll();
    }

    [TestCaseSource(nameof(AddOrderAsyncData))]
    public async Task AddOrderAsync_ValidData_ReturnsOrderId(ModelsBriefOrder order, long orderId)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.AddOrderAsync(It.IsAny<RepositoryOrder>()))
            .ReturnsAsync(orderId)
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<ModelsAddOrder> actualResult = await controller.AddOrderAsync(order: order);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var actualResultValue = ((OkObjectResult)actualResult.Result!).Value;
        Assert.That(actualResultValue, Is.Not.Null);
        Assert.That(actualResultValue, Is.InstanceOf<ModelsAddOrder>());
        Assert.That(((ModelsAddOrder)actualResultValue!).OrderId, Is.EqualTo(orderId));

        orderRepositoryMock.VerifyAll();
    }

    [TestCaseSource(nameof(AddOrderAsyncData))]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Discard parameter")]
    public async Task AddOrderAsync_ThrowsException_ReturnsServerError(ModelsBriefOrder order, long _)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.AddOrderAsync(It.IsAny<RepositoryOrder>()))
            .ThrowsAsync(new Exception())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult<ModelsAddOrder> actualResult = await controller.AddOrderAsync(order: order);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult.Result, Is.Not.Null);
        Assert.That(actualResult.Value, Is.Null);
        Assert.That(actualResult.Result, Is.InstanceOf(typeof(StatusCodeResult)));
        Assert.That(((StatusCodeResult)actualResult.Result!).StatusCode, Is.EqualTo(500));

        orderRepositoryMock.VerifyAll();
    }

    [TestCase(10248)]
    public async Task RemoveOrderAsync_OrderIsExist_RemovesOrder(long orderId)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.RemoveOrderAsync(It.Is<long>(arg => arg == orderId)))
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult actualResult = await controller.RemoveOrderAsync(orderId: orderId);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult, Is.InstanceOf(typeof(NoContentResult)));

        orderRepositoryMock.VerifyAll();
    }

    [TestCase(1)]
    public async Task RemoveOrderAsync_OrderIsNotExist_ReturnsNotFound(long orderId)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.RemoveOrderAsync(It.Is<long>((arg) => arg == orderId)))
            .ThrowsAsync(new OrderNotFoundException())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult actualResult = await controller.RemoveOrderAsync(orderId: orderId);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult, Is.InstanceOf(typeof(NotFoundResult)));

        orderRepositoryMock.VerifyAll();
    }

    [TestCase(10248)]
    public async Task RemoveOrderAsync_ExceptionThrown_ReturnsServerError(long orderId)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.RemoveOrderAsync(It.Is<long>((arg) => arg == orderId)))
            .ThrowsAsync(new Exception())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult actualResult = await controller.RemoveOrderAsync(orderId: orderId);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult, Is.InstanceOf(typeof(StatusCodeResult)));
        Assert.That(((StatusCodeResult)actualResult).StatusCode, Is.EqualTo(500));

        orderRepositoryMock.VerifyAll();
    }

    [TestCaseSource(nameof(UpdateOrderAsyncData))]
    public async Task UpdateOrderAsync_OrderIsExist_UpdatesOrder(long orderId, ModelsBriefOrder order)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.UpdateOrderAsync(It.Is<RepositoryOrder>(arg => arg.Id == orderId)))
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult actualResult = await controller.UpdateOrderAsync(orderId: orderId, order: order);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult, Is.InstanceOf(typeof(NoContentResult)));

        orderRepositoryMock.VerifyAll();
    }

    [TestCaseSource(nameof(UpdateOrderAsyncData))]
    public async Task UpdateOrderAsync_OrderIsNotExist_ReturnsNotFound(long orderId, ModelsBriefOrder order)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.UpdateOrderAsync(It.Is<RepositoryOrder>((arg) => arg.Id == orderId)))
            .ThrowsAsync(new OrderNotFoundException())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult actualResult = await controller.UpdateOrderAsync(orderId: orderId, order: order);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult, Is.InstanceOf(typeof(NotFoundResult)));

        orderRepositoryMock.VerifyAll();
    }

    [TestCaseSource(nameof(UpdateOrderAsyncData))]
    public async Task UpdateOrderAsync_ExceptionThrown_ReturnsServerError(long orderId, ModelsBriefOrder order)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.UpdateOrderAsync(It.Is<RepositoryOrder>((arg) => arg.Id == orderId)))
            .ThrowsAsync(new Exception())
            .Verifiable();

        var loggerMock = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(orderRepositoryMock.Object, loggerMock.Object);

        // Act
        ActionResult actualResult = await controller.UpdateOrderAsync(orderId: orderId, order: order);

        // Assert
        Assert.That(actualResult, Is.Not.Null);
        Assert.That(actualResult, Is.InstanceOf(typeof(StatusCodeResult)));
        Assert.That(((StatusCodeResult)actualResult).StatusCode, Is.EqualTo(500));

        orderRepositoryMock.VerifyAll();
    }

    [TestCase(nameof(OrdersController.GetOrderAsync))]
    [TestCase(nameof(OrdersController.GetOrdersAsync))]
    public void HasHttpGetAction(string methodName)
    {
        _ = AssertMethodHasAttribute<HttpGetAttribute>(methodName);
    }

    [TestCase(nameof(OrdersController.AddOrderAsync))]
    public void HasHttpPostAction(string methodName)
    {
        _ = AssertMethodHasAttribute<HttpPostAttribute>(methodName);
    }

    [TestCase(nameof(OrdersController.UpdateOrderAsync))]
    public void HasHttpPutAction(string methodName)
    {
        _ = AssertMethodHasAttribute<HttpPutAttribute>(methodName);
    }

    [TestCase(nameof(OrdersController.RemoveOrderAsync))]
    public void HasHttpDeleteAction(string methodName)
    {
        _ = AssertMethodHasAttribute<HttpDeleteAttribute>(methodName);
    }

    [Test]
    public void IsPublicСlass()
    {
        var controllerType = typeof(OrdersController);
        Assert.That(controllerType.IsClass, Is.True);
        Assert.That(controllerType.IsPublic, Is.True);
        Assert.That(controllerType.IsAbstract, Is.False);
    }

    [TestCase(typeof(ApiControllerAttribute))]
    [TestCase(typeof(RouteAttribute))]
    public void HasAttribute(Type attributeType)
    {
        var controllerType = typeof(OrdersController);
        var attribute = controllerType.GetCustomAttribute(attributeType);
        Assert.That(attribute, Is.Not.Null);
    }

    [Test]
    public void InheritsControllerBase()
    {
        var controllerType = typeof(OrdersController);
        Assert.That(controllerType.BaseType, Is.EqualTo(typeof(ControllerBase)));
    }

    [TestCaseSource(nameof(ConstructorData))]
    public void HasPublicConstructor(Type[] parameterTypes)
    {
        var controllerType = typeof(OrdersController);
        var constructorInfo = controllerType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, parameterTypes, null);
        Assert.That(constructorInfo, Is.Not.Null);
    }

    private static T AssertMethodHasAttribute<T>(string methodName)
        where T : Attribute
    {
        var controllerType = typeof(OrdersController);
        var methodInfo = controllerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(methodInfo, Is.Not.Null);

        var attribute = methodInfo!.GetCustomAttribute<T>(false);
        Assert.That(attribute, Is.Not.Null);

        return attribute!;
    }

    private static IEnumerable<object[]> GetOrderAsyncData()
    {
        var repositoryOrder = new RepositoryOrder(10248)
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

        repositoryOrder.OrderDetails.Add(new RepositoryOrderDetail(repositoryOrder)
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

        repositoryOrder.OrderDetails.Add(new RepositoryOrderDetail(repositoryOrder)
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

        repositoryOrder.OrderDetails.Add(new RepositoryOrderDetail(repositoryOrder)
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

        var expectedOrder = new ModelsFullOrder
        {
            Id = 10248,
            Customer = new ModelsCustomer
            {
                Code = "VINET",
                CompanyName = "Vins et alcools Chevalier",
            },
            Employee = new ModelsEmployee
            {
                Id = 5,
                FirstName = "Steven",
                LastName = "Buchanan",
                Country = "UK",
            },
            OrderDate = new DateTime(1996, 7, 4),
            RequiredDate = new DateTime(1996, 8, 1),
            ShippedDate = new DateTime(1996, 7, 16),
            Freight = 32.38,
            ShipName = "Vins et alcools Chevalier",
            Shipper = new ModelsShipper
            {
                Id = 3,
                CompanyName = "Federal Shipping",
            },
            ShippingAddress = new ModelsShippingAddress
            {
                Address = "59 rue de l'Abbaye",
                City = "Reims",
                Region = null,
                PostalCode = "51100",
                Country = "France",
            },
        };

        yield return new object[] { repositoryOrder, expectedOrder };
    }

    private static IEnumerable<object[]> GetOrdersAsyncData()
    {
        var repositoryOrder = new RepositoryOrder(10248)
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

        repositoryOrder.OrderDetails.Add(new RepositoryOrderDetail(repositoryOrder)
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

        repositoryOrder.OrderDetails.Add(new RepositoryOrderDetail(repositoryOrder)
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

        repositoryOrder.OrderDetails.Add(new RepositoryOrderDetail(repositoryOrder)
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

        var expectedOrder = new ModelsBriefOrder
        {
            Id = 10248,
            CustomerId = "VINET",
            EmployeeId = 5,
            OrderDate = new DateTime(1996, 7, 4),
            RequiredDate = new DateTime(1996, 8, 1),
            ShippedDate = new DateTime(1996, 7, 16),
            Freight = 32.38,
            ShipName = "Vins et alcools Chevalier",
            ShipperId = 3,
            ShipAddress = "59 rue de l'Abbaye",
            ShipCity = "Reims",
            ShipRegion = null,
            ShipPostalCode = "51100",
            ShipCountry = "France",
        };

        yield return new object[] { new List<RepositoryOrder> { repositoryOrder }, 0, 1, new List<ModelsBriefOrder> { expectedOrder } };
    }

    private static IEnumerable<object[]> AddOrderAsyncData()
    {
        ModelsBriefOrder order = new ModelsBriefOrder
        {
            Id = -1,
            CustomerId = "VINET",
            EmployeeId = 5,
            ShipperId = 3,
            OrderDate = new DateTime(1996, 7, 4),
            RequiredDate = new DateTime(1996, 8, 1),
            ShippedDate = new DateTime(1996, 7, 16),
            Freight = 32.38,
            ShipName = "Vins et alcools Chevalier",
            ShipAddress = "59 rue de l'Abbaye",
            ShipCity = "Reims",
            ShipRegion = null,
            ShipPostalCode = "51100",
            ShipCountry = "France",
            OrderDetails = new List<ModelsBriefOrderDetail>(),
        };

        order.OrderDetails.Add(new ModelsBriefOrderDetail
        {
            ProductId = 11,
            UnitPrice = 14,
            Quantity = 12,
            Discount = 0,
        });

        order.OrderDetails.Add(new ModelsBriefOrderDetail
        {
            ProductId = 42,
            UnitPrice = 9.8,
            Quantity = 10,
            Discount = 0,
        });

        order.OrderDetails.Add(new ModelsBriefOrderDetail
        {
            ProductId = 72,
            UnitPrice = 34.8,
            Quantity = 5,
            Discount = 0,
        });

        yield return new object[] { order, 10248L };
    }

    private static IEnumerable<object[]> UpdateOrderAsyncData()
    {
        ModelsBriefOrder order = new ModelsBriefOrder
        {
            Id = -1,
            CustomerId = "ALFKI",
            EmployeeId = 1,
            OrderDate = new DateTime(1990, 1, 2),
            RequiredDate = new DateTime(1991, 3, 4),
            ShippedDate = new DateTime(1993, 5, 6),
            Freight = 123_456.12,
            ShipperId = 1,
            ShipName = "Alfreds Futterkiste",
            ShipAddress = "address",
            ShipCity = "city",
            ShipRegion = "region",
            ShipPostalCode = "postal code",
            ShipCountry = "country",
            OrderDetails = new List<ModelsBriefOrderDetail>(),
        };

        order.OrderDetails.Add(new ModelsBriefOrderDetail
        {
            ProductId = 1,
            UnitPrice = 123_456,
            Quantity = 123_456,
            Discount = 1,
        });

        yield return new object[] { 10248L, order };
    }
}
