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

        public Task<IList<RepositoryOrder>> GetOrdersAsync(int skip, int count)
        {
            throw new NotImplementedException();
        }

        public Task<long> AddOrderAsync(RepositoryOrder order)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveOrderAsync(long orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(od => od.Id == orderId);

            if (order == null)
            {
                throw new NotImplementedException();
            }

            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();

            _context.OrderDetails.RemoveRange(orderDetails);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public Task UpdateOrderAsync(RepositoryOrder order)
        {
            throw new NotImplementedException();
        }
    }
}
