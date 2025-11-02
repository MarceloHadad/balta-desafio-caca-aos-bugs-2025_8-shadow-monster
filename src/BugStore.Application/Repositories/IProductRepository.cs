using BugStore.Application.UseCases.Products.Search;
using BugStore.Domain.Entities;

namespace BugStore.Application.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(SearchProductsRequest request);
}
