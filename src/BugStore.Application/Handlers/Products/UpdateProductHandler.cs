using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Products;

namespace BugStore.Application.Handlers.Products;

public class UpdateProductHandler : IHandler<UpdateProductRequest, UpdateProductResponse>
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _uow;

    public UpdateProductHandler(IProductRepository products, IUnitOfWork uow)
    {
        _products = products;
        _uow = uow;
    }

    public async Task<UpdateProductResponse> HandleAsync(UpdateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Description is required");
        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Slug is required");
        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero");

        var existing = await _products.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Product not found");

        var slugOwner = await _products.GetBySlugAsync(request.Slug);
        if (slugOwner is not null && slugOwner.Id != request.Id)
            throw new InvalidOperationException("Slug already in use");

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.Slug = request.Slug;
        existing.Price = request.Price;

        await _products.UpdateAsync(existing);
        await _uow.CommitAsync();

        return new UpdateProductResponse
        {
            Id = existing.Id,
            Title = existing.Title,
            Description = existing.Description,
            Slug = existing.Slug,
            Price = existing.Price
        };
    }
}
