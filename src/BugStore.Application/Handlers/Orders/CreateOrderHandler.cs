using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Orders;
using BugStore.Application.Responses.Orders;
using BugStore.Domain.Entities;

namespace BugStore.Application.Handlers.Orders;

public class CreateOrderHandler : IHandler<CreateOrderRequest, CreateOrderResponse>
{
    private readonly IOrderRepository _orders;
    private readonly IOrderLineRepository _orderLines;
    private readonly IProductRepository _products;
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _uow;

    public CreateOrderHandler(
        IOrderRepository orders,
        IOrderLineRepository orderLines,
        IProductRepository products,
        ICustomerRepository customers,
        IUnitOfWork uow)
    {
        _orders = orders;
        _orderLines = orderLines;
        _products = products;
        _customers = customers;
        _uow = uow;
    }

    public async Task<CreateOrderResponse> HandleAsync(CreateOrderRequest request)
    {
        if (request.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required");
        if (request.Lines is null || request.Lines.Count == 0)
            throw new ArgumentException("Order must have at least one line");

        var customer = await _customers.GetByIdAsync(request.CustomerId)
            ?? throw new KeyNotFoundException("Customer not found");

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
        }

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _products.GetByIdsAsync(productIds);
        if (products.Count != productIds.Count)
            throw new KeyNotFoundException("One or more products not found");

        var productsById = products.ToDictionary(p => p.Id);

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = now,
            UpdatedAt = now,
            Lines = []
        };

        var lines = new List<OrderLine>();
        foreach (var lineRequest in request.Lines)
        {
            var product = productsById[lineRequest.ProductId];
            var total = product.Price * lineRequest.Quantity;

            lines.Add(new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = lineRequest.Quantity,
                Total = total
            });
        }

        order.Lines = lines;
        await _orders.AddAsync(order);
        await _orderLines.AddRangeAsync(lines);
        await _uow.CommitAsync();

        return new CreateOrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Lines = lines.Select(l => new OrderLineResponse
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductTitle = productsById[l.ProductId].Title,
                Quantity = l.Quantity,
                UnitPrice = productsById[l.ProductId].Price,
                Total = l.Total
            }).ToList()
        };
    }
}
