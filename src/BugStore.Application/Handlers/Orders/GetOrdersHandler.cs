using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Responses.Orders;
using BugStore.Application.UseCases.Orders.Search;

namespace BugStore.Application.Handlers.Orders;

public class GetOrdersHandler : IHandler<SearchOrdersRequest, GetOrdersResponse>
{
    private readonly IOrderRepository _repository;

    public GetOrdersHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetOrdersResponse> HandleAsync(SearchOrdersRequest request)
    {
        var (orders, totalCount) = await _repository.SearchAsync(request);

        var items = orders.Select(o => new GetByIdOrderResponse
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer.Name,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            TotalAmount = o.Lines.Sum(l => l.Total),
            Lines = o.Lines.Select(l => new OrderLineResponse
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductTitle = l.Product.Title,
                Quantity = l.Quantity,
                UnitPrice = l.Product.Price,
                Total = l.Total
            }).ToList()
        }).ToList();

        return new GetOrdersResponse
        {
            Orders = items,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10,
            TotalCount = totalCount
        };
    }
}
