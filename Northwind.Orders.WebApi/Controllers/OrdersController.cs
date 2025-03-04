using Microsoft.AspNetCore.Mvc;
using Northwind.Orders.WebApi.Models;
using Northwind.Services.Repositories;
using ModFullOrder = Northwind.Orders.WebApi.Models.FullOrder;
using ModCustomer = Northwind.Orders.WebApi.Models.Customer;
using ModEmployee = Northwind.Orders.WebApi.Models.Employee;
using ModShipper = Northwind.Orders.WebApi.Models.Shipper;
using ModShippingAddress = Northwind.Orders.WebApi.Models.ShippingAddress;
using RepoOrderDetails = Northwind.Services.Repositories.OrderDetail;
using RepoProduct = Northwind.Services.Repositories.Product;
using RepoOrder = Northwind.Services.Repositories.Order;
using RepoCustomer = Northwind.Services.Repositories.Customer;
using RepoEmployee = Northwind.Services.Repositories.Employee;
using RepoShipper = Northwind.Services.Repositories.Shipper;
using RepoShippingAddress = Northwind.Services.Repositories.ShippingAddress;
using RepoCustomerCode = Northwind.Services.Repositories.CustomerCode;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.EntityFramework.Repositories;

namespace Northwind.Orders.WebApi.Controllers;

public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<ActionResult<FullOrder>> GetOrderAsync(long orderId)
    {
        try
        {
            var repoorder = await _orderRepository.GetOrderAsync(orderId);
            var fullorder = new FullOrder()
            {
                Id = orderId,
                Customer = new ModCustomer()
                {
                    Code = repoorder.Customer.Code.Code,
                    CompanyName = repoorder.Customer.CompanyName,
                },
                Employee = new ModEmployee()
                {
                    Id = repoorder.Employee.Id,
                    FirstName = repoorder.Employee.FirstName,
                    LastName = repoorder.Employee.LastName,
                    Country = repoorder.Employee.Country,
                },
                OrderDate = repoorder.OrderDate,
                RequiredDate = repoorder.RequiredDate,
                ShippedDate = repoorder.ShippedDate,
                Shipper = new ModShipper()
                {
                    Id = repoorder.Shipper.Id,
                    CompanyName = repoorder.Shipper.CompanyName,
                },
                Freight = repoorder.Freight,
                ShipName = repoorder.ShipName,
                ShippingAddress = new ModShippingAddress()
                {
                    Address = repoorder.ShippingAddress.Address,
                    City = repoorder.ShippingAddress.City,
                    Region = repoorder.ShippingAddress.Region,
                    PostalCode = repoorder.ShippingAddress.PostalCode,
                    Country = repoorder.ShippingAddress.Country,
                },
            };
            var fullOrderDetailLisr = repoorder.OrderDetails.Select(repod => new FullOrderDetail()
            {
                ProductId = repod.Product.Id,
                ProductName = repod.Product.ProductName,
                CategoryId = repod.Product.CategoryId,
                CategoryName = repod.Product.ProductName,
                SupplierId = repod.Product.SupplierId,
                SupplierCompanyName = repod.Product.Supplier,
                UnitPrice = repod.UnitPrice,
                Quantity = repod.Quantity,
                Discount = repod.Discount,
            }).ToList();

            fullorder.OrderDetails = fullOrderDetailLisr;

            _logger.LogInformation("Successfully mapped order with ID {OrderId} to FullOrder.", orderId);

            return Ok(fullorder);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID {OrderId} not found.", orderId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting order {OrderId}", orderId);
            return StatusCode(500);
        }
    }

    public Task<ActionResult<IEnumerable<BriefOrder>>> GetOrdersAsync(int? skip, int? count)
    {
        throw new Exception();
    }

    public async Task<ActionResult<AddOrder>> AddOrderAsync(BriefOrder order)
    {
        try
        {
            var customercode = new RepoCustomerCode(order.CustomerId);
            var repoOrder = new RepoOrder(order.Id)
            {
                Customer = new RepoCustomer(customercode),
                Employee = new RepoEmployee(order.EmployeeId),
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                Shipper = new RepoShipper(order.ShipperId),
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShippingAddress = new RepoShippingAddress(order.ShipAddress, order.ShipCity, order.ShipRegion, order.ShipPostalCode, order.ShipCountry),
            };

            var orderDetailsList = order.OrderDetails.Select(od => new RepoOrderDetails(repoOrder)
            {
                Product = new RepoProduct(od.ProductId),
                UnitPrice = od.UnitPrice,
                Quantity = od.Quantity,
                Discount = od.Discount,
            }).ToList();

            repoOrder.OrderDetails = orderDetailsList;

            var resultId = await _orderRepository.AddOrderAsync(repoOrder);

            _logger.LogInformation("Order added successfully with ID {OrderId}", resultId);
            var orderId = new AddOrder() { OrderId = resultId };

            return Ok(orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding order");
            return new StatusCodeResult(500);
        }
    }

    public async Task<ActionResult> RemoveOrderAsync(long orderId)
    {
        try
        {
            _logger.LogInformation("Attempting to remove order with ID: {OrderId}", orderId);
            await _orderRepository.RemoveOrderAsync(orderId);
            _logger.LogInformation("Successfully removed order with ID: {OrderId}", orderId);
            return NoContent();
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID: {OrderId} not found", orderId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while removing order with ID: {OrderId}", orderId);
            return StatusCode(500);
        }
    }

    public async Task<ActionResult> UpdateOrderAsync(long orderId, BriefOrder order)
    {
        try
        {
            var customercode = new RepoCustomerCode(order.CustomerId);
            var repoOrder = new RepoOrder(orderId)
            {
                Customer = new RepoCustomer(customercode),
                Employee = new RepoEmployee(order.EmployeeId),
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                Shipper = new RepoShipper(order.ShipperId),
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShippingAddress = new RepoShippingAddress(order.ShipAddress, order.ShipCity, order.ShipRegion, order.ShipPostalCode, order.ShipCountry),
            };

            var orderDetailsList = order.OrderDetails.Select(od => new RepoOrderDetails(repoOrder)
            {
                Product = new RepoProduct(od.ProductId),
                UnitPrice = od.UnitPrice,
                Quantity = od.Quantity,
                Discount = od.Discount,
            }).ToList();

            repoOrder.OrderDetails = orderDetailsList;

            await _orderRepository.UpdateOrderAsync(repoOrder);

            return NoContent();
        }
        catch (OrderNotFoundException)
        {
            _logger.LogWarning("Order with ID {OrderId} not found.", orderId);

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update order with ID {OrderId}", orderId);

            return StatusCode(500);
        }
    }
}
