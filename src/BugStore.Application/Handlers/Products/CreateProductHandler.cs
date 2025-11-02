using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Products;
using BugStore.Domain.Entities;

namespace BugStore.Application.Handlers.Products;

public class CreateProductHandler : IHandler<CreateProductRequest, CreateProductResponse>
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _uow;

    public CreateProductHandler(IProductRepository products, IUnitOfWork uow)
    {
        _products = products;
        _uow = uow;
    }

    public async Task<CreateProductResponse> HandleAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Description is required");
        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Slug is required");
        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero");

        var slugInUse = await _products.GetBySlugAsync(request.Slug) != null;
        if (slugInUse)
            throw new InvalidOperationException("Slug already in use");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Slug = request.Slug,
            Price = request.Price
        };

        await _products.AddAsync(product);
        await _uow.CommitAsync();

        return new CreateProductResponse
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price
        };
    }
}
