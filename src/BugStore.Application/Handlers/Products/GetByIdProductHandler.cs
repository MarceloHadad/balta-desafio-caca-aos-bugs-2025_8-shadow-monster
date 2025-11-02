using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Products;

namespace BugStore.Application.Handlers.Products;

public class GetByIdProductHandler : IHandler<GetByIdProductRequest, GetByIdProductResponse>
{
    private readonly IProductRepository _products;

    public GetByIdProductHandler(IProductRepository products)
    {
        _products = products;
    }

    public async Task<GetByIdProductResponse> HandleAsync(GetByIdProductRequest request)
    {
        var product = await _products.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Product not found");

        return new GetByIdProductResponse
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price
        };
    }
}
