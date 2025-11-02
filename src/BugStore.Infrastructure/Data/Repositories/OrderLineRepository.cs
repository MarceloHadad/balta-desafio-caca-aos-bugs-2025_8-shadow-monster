using BugStore.Application.Repositories;
using BugStore.Domain.Entities;

namespace BugStore.Infrastructure.Data.Repositories;

public class OrderLineRepository(AppDbContext context) : IOrderLineRepository
{
    private readonly AppDbContext _context = context;

    public Task AddRangeAsync(IEnumerable<OrderLine> lines)
    {
        _context.OrderLines.AddRange(lines);
        return Task.CompletedTask;
    }

    public void DeleteRange(IEnumerable<OrderLine> lines)
    {
        _context.OrderLines.RemoveRange(lines);
    }
}
