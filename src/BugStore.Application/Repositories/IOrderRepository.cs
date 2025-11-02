using BugStore.Application.UseCases.Orders.Search;
using BugStore.Domain.Entities;

namespace BugStore.Application.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task AddAsync(Order order);
    Task Update(Order order);
    Task Delete(Order order);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchAsync(SearchOrdersRequest request);
}
