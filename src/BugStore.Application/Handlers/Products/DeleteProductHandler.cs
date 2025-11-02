using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Products;

namespace BugStore.Application.Handlers.Products;

public class DeleteProductHandler : IHandler<DeleteProductRequest, DeleteProductResponse>
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _uow;

    public DeleteProductHandler(IProductRepository products, IUnitOfWork uow)
    {
        _products = products;
        _uow = uow;
    }

    public async Task<DeleteProductResponse> HandleAsync(DeleteProductRequest request)
    {
        var exists = await _products.GetByIdAsync(request.Id) != null;
        if (!exists)
            throw new KeyNotFoundException("Product not found");

        await _products.DeleteAsync(request.Id);
        await _uow.CommitAsync();

        return new DeleteProductResponse();
    }
}
