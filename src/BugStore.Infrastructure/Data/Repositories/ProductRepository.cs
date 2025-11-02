using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Products.Search;
using BugStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Data.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        return await Task.FromResult(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found");
        _context.Products.Remove(entity);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        return await _context.Products
            .AsNoTracking()
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task UpdateAsync(Product product)
    {
        var tracked = await _context.Products.FindAsync(product.Id)
            ?? throw new KeyNotFoundException("Product not found");

        tracked.Title = product.Title;
        tracked.Description = product.Description;
        tracked.Slug = product.Slug;
        tracked.Price = product.Price;
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(SearchProductsRequest request)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var value = request.Title.Trim().ToLower();
            query = query.Where(p => EF.Functions.Like(p.Title.ToLower(), $"%{value}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var value = request.Description.Trim().ToLower();
            query = query.Where(p => EF.Functions.Like(p.Description.ToLower(), $"%{value}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var value = request.Slug.Trim().ToLower();
            query = query.Where(p => EF.Functions.Like(p.Slug.ToLower(), $"%{value}%"));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        var pageNumber = (request.PageNumber ?? 1);
        if (pageNumber < 1) pageNumber = 1;
        var pageSize = (request.PageSize ?? 10);
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
