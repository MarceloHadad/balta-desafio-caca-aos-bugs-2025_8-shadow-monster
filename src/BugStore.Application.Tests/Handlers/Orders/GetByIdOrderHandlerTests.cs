using BugStore.Application.Handlers.Orders;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Orders;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Orders;

public class GetByIdOrderHandlerTests
{
    private readonly GetByIdOrderHandler _handler;
    private readonly Mock<IOrderRepository> _repo;

    public GetByIdOrderHandlerTests()
    {
        _repo = new Mock<IOrderRepository>();
        _handler = new GetByIdOrderHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderExists_ReturnsResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new GetByIdOrderRequest(orderId);
        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Customer = new Customer
            {
                Id = customerId,
                Name = "Jane Doe"
            },
            Lines =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 2,
                    Total = 100.00m,
                    Product = new Product
                    {
                        Id = productId,
                        Title = "Product 1",
                        Price = 50.00m
                    }
                }
            ]
        };

        _repo.Setup(r => r.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(orderId);
        response.CustomerId.Should().Be(customerId);
        response.CustomerName.Should().Be("Jane Doe");
        response.TotalAmount.Should().Be(100.00m);
        response.Lines.Should().HaveCount(1);
        response.Lines[0].ProductId.Should().Be(productId);
        response.Lines[0].ProductTitle.Should().Be("Product 1");
        response.Lines[0].Quantity.Should().Be(2);
        response.Lines[0].UnitPrice.Should().Be(50.00m);
        response.Lines[0].Total.Should().Be(100.00m);

        _repo.Verify(r => r.GetByIdWithDetailsAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new GetByIdOrderRequest(orderId);

        _repo.Setup(r => r.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Order not found");

        _repo.Verify(r => r.GetByIdWithDetailsAsync(orderId), Times.Once);
    }
}
