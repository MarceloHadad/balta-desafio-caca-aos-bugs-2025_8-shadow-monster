using BugStore.Domain.Entities;

namespace BugStore.Application.Repositories;

public interface IOrderLineRepository
{
    Task AddRangeAsync(IEnumerable<OrderLine> lines);
    void DeleteRange(IEnumerable<OrderLine> lines);
}
