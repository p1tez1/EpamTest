using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.Repositories;
using RepositoryOrder = Northwind.Services.Repositories.Order;
using RepoCustomer = Northwind.Services.Repositories.Customer;
using RepoEmployee = Northwind.Services.Repositories.Employee;
using RepoOrderDetail = Northwind.Services.Repositories.OrderDetail;
using RepoShipper = Northwind.Services.Repositories.Shipper;
using RepoShippingAddress = Northwind.Services.Repositories.ShippingAddress;
using RepoProduct = Northwind.Services.Repositories.Product;

namespace Northwind.Services.EntityFramework.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly NorthwindContext _context;

        public OrderRepository(NorthwindContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RepositoryOrder> GetOrderAsync(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Supplier)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new OrderNotFoundException();
            }

            var customercode = new CustomerCode(order.CustomerId);
            var repoOrder = new RepositoryOrder(order.Id)
            {
                Customer = new RepoCustomer(customercode)
                {
                    CompanyName = order.Customer.CompanyName,
                },
                Employee = new RepoEmployee(order.Employee.Id)
                {
                    FirstName = order.Employee.FirstName,
                    LastName = order.Employee.LastName,
                    Country = order.Employee.Country,
                },
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                Shipper = new RepoShipper(order.Shipper.Id)
                {
                    CompanyName = order.Shipper.CompanyName,
                },
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShippingAddress = new RepoShippingAddress(
                    order.ShipAddress, order.ShipCity, order.ShipRegion, order.ShipPostalCode, order.ShipCountry),
            };

            var orderDetailsList = order.OrderDetails.Select(od => new RepoOrderDetail(repoOrder)
            {
                Product = new RepoProduct(od.ProductId)
                {
                    ProductName = od.Product.ProductName,
                    Supplier = od.Product.Supplier.CompanyName,
                    SupplierId = od.Product.SupplierId,
                    Category = od.Product.Category.CategoryName,
                    CategoryId = od.Product.CategoryId,
                },
                UnitPrice = od.UnitPrice,
                Quantity = od.Quantity,
                Discount = od.Discount,
            }).ToList();

            repoOrder.OrderDetails = orderDetailsList;

            return repoOrder;
        }

        public async Task<IList<RepositoryOrder>> GetOrdersAsync(int skip, int count)
        {
            throw new Exception();
        }

        public async Task<long> AddOrderAsync(RepositoryOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                throw new RepositoryException("Order must contain at least one order detail");
            }

            var customerId = order.Customer.Code;
            var entityOrder = new Entities.Order()
            {
                Id = order.Id,
                CustomerId = customerId.Code,
                ShipperId = order.Shipper.Id,
                EmployeeId = order.Employee.Id,
                OrderDate = order.OrderDate,
                RequiredDate = order.OrderDate,
                ShippedDate = order.OrderDate,
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShipAddress = order.ShippingAddress.Address,
                ShipCity = order.ShippingAddress.City,
                ShipRegion = order.ShippingAddress.Region,
                ShipPostalCode = order.ShippingAddress.PostalCode,
                ShipCountry = order.ShippingAddress.Country,
                Employee = new Entities.Employee()
                {
                    Id = order.Employee.Id,
                    LastName = order.Employee.LastName,
                    FirstName = order.Employee.FirstName,
                    Country = order.Employee.Country,
                },
                Shipper = new Entities.Shipper()
                {
                    Id = order.Shipper.Id,
                    CompanyName = order.Shipper.CompanyName,
                },
                Customer = new Entities.Customer()
                {
                    Id = customerId.Code,
                    CompanyName = order.Customer.CompanyName,
                },
            };

            var orderDetailList = order.OrderDetails.Select(rod => new Entities.OrderDetail()
            {
                OrderId = rod.Order.Id,
                ProductId = rod.Product.Id,
                UnitPrice = rod.UnitPrice,
                Quantity = rod.Quantity,
                Discount = rod.Discount,
            }).ToList();

            entityOrder.OrderDetails = orderDetailList;

            try
            {
                _context.Add(entityOrder);
                await _context.SaveChangesAsync();

                return entityOrder.Id;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to add order: {ex.Message}", ex);
            }
        }

        public async Task RemoveOrderAsync(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(od => od.Id == orderId);

            if (order == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }

            _context.OrderDetails.RemoveRange(order.OrderDetails);

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(RepositoryOrder order)
        {
            var editingOrder = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Supplier)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (editingOrder == null)
            {
                throw new OrderNotFoundException();
            }

            editingOrder.CustomerId = order.Customer.Code.Code;
            editingOrder.ShipperId = order.Shipper.Id;
            editingOrder.EmployeeId = order.Employee.Id;
            editingOrder.OrderDate = order.OrderDate;
            editingOrder.RequiredDate = order.RequiredDate;
            editingOrder.ShippedDate = order.ShippedDate;
            editingOrder.Freight = order.Freight;
            editingOrder.ShipName = order.ShipName;
            editingOrder.ShipAddress = order.ShippingAddress.Address;
            editingOrder.ShipCity = order.ShippingAddress.City;
            editingOrder.ShipRegion = order.ShippingAddress.Region;
            editingOrder.ShipPostalCode = order.ShippingAddress.PostalCode;
            editingOrder.ShipCountry = order.ShippingAddress.Country;
            editingOrder.Employee.Id = order.Employee.Id;
            editingOrder.Employee.FirstName = order.Employee.FirstName;
            editingOrder.Employee.LastName = order.Employee.LastName;
            editingOrder.Employee.Country = order.Employee.Country;
            editingOrder.Shipper.Id = order.Shipper.Id;
            editingOrder.Shipper.CompanyName = order.Shipper.CompanyName;
            editingOrder.Customer.Id = order.Customer.Code.Code;
            editingOrder.Customer.CompanyName = order.Customer.CompanyName;

            var orderDetailList = order.OrderDetails.Select(rod => new Entities.OrderDetail()
            {
                OrderId = rod.Order.Id,
                ProductId = rod.Product.Id,
                UnitPrice = rod.UnitPrice,
                Quantity = rod.Quantity,
                Discount = rod.Discount,
            }).ToList();
        }
    }
}
