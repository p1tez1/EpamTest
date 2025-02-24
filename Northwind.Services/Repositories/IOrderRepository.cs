namespace Northwind.Services.Repositories;

/// <summary>
/// Represents a repository for storing a collection of objects of the <see cref="Order"/> type.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets a list of orders from the repository.
    /// </summary>
    /// <param name="skip">The number of orders to skip before adding an order to the result list.</param>
    /// <param name="count">The number of orders in the result list.</param>
    /// <returns>A <see cref="Task{TResult}"/>that wraps a list of orders.</returns>
    /// <remarks>The result list should be sorted by order id from smallest to largest.</remarks>
    Task<IList<Order>> GetOrdersAsync(int skip, int count);

    /// <summary>
    /// Gets an order with the specified identifier from the repository.
    /// </summary>
    /// <param name="orderId">The identifier of an order to return.</param>
    /// <returns>A <see cref="Task{TResult}"/> that wraps an order with the specified identifier.</returns>
    Task<Order> GetOrderAsync(long orderId);

    /// <summary>
    /// Adds an order to the repository.
    /// </summary>
    /// <param name="order">An <see cref="Order"/>.</param>
    /// <returns>The identifier of an order to add.</returns>
    /// <returns>A <see cref="Task{T}"/> that wraps an identifier of an order that was added to the repository.</returns>
    Task<long> AddOrderAsync(Order order);

    /// <summary>
    /// Removes an order with the specified identifier from the repository.
    /// </summary>
    /// <param name="orderId">The identifier of an order to remove.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task RemoveOrderAsync(long orderId);

    /// <summary>
    /// Updates an order data with the specified identifier in the repository.
    /// </summary>
    /// <param name="order">The identifier of an order to update.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task UpdateOrderAsync(Order order);
}
