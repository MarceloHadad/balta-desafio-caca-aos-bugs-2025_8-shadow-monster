using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Orders;
using BugStore.Application.Responses.Orders;

namespace BugStore.Application.Handlers.Orders;

public class GetByIdOrderHandler : IHandler<GetByIdOrderRequest, GetByIdOrderResponse>
{
    private readonly IOrderRepository _orders;

    public GetByIdOrderHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<GetByIdOrderResponse> HandleAsync(GetByIdOrderRequest request)
    {
        var order = await _orders.GetByIdWithDetailsAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found");

        return new GetByIdOrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Name,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            TotalAmount = order.Lines.Sum(l => l.Total),
            Lines = order.Lines.Select(l => new OrderLineResponse
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductTitle = l.Product.Title,
                Quantity = l.Quantity,
                UnitPrice = l.Product.Price,
                Total = l.Total
            }).ToList()
        };
    }
}
