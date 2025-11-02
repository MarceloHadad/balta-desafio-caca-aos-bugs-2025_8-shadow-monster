using BugStore.Application.Handlers.Orders;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Orders;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Orders;

public class CreateOrderHandlerTests
{
    private readonly CreateOrderHandler _handler;
    private readonly Mock<IOrderRepository> _orders;
    private readonly Mock<IOrderLineRepository> _orderLines;
    private readonly Mock<IProductRepository> _products;
    private readonly Mock<ICustomerRepository> _customers;
    private readonly Mock<IUnitOfWork> _uow;

    public CreateOrderHandlerTests()
    {
        _orders = new Mock<IOrderRepository>();
        _orderLines = new Mock<IOrderLineRepository>();
        _products = new Mock<IProductRepository>();
        _customers = new Mock<ICustomerRepository>();
        _uow = new Mock<IUnitOfWork>();
        _handler = new CreateOrderHandler(
            _orders.Object,
            _orderLines.Object,
            _products.Object,
            _customers.Object,
            _uow.Object
        );
    }

    [Fact]
    public async Task HandleAsync_WhenValidRequest_PersistsAndReturnsResponse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Lines =
            [
                new() { ProductId = productId, Quantity = 2 }
            ]
        };
        var customer = new Customer
        {
            Id = customerId,
            Name = "Jane Doe",
            Email = "jane@example.com"
        };
        var product = new Product
        {
            Id = productId,
            Title = "Product 1",
            Price = 50.00m
        };

        _customers.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _products.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Product> { product });

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.CustomerId.Should().Be(customerId);
        response.Lines.Should().HaveCount(1);
        response.Lines[0].ProductId.Should().Be(productId);
        response.Lines[0].Quantity.Should().Be(2);
        response.Lines[0].Total.Should().Be(100.00m);

        _customers.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _products.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderLines.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<OrderLine>>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.Empty,
            Lines =
            [
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            ]
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("CustomerId is required");

        _customers.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenLinesIsNull_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Lines = null!
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Order must have at least one line");

        _customers.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenLinesIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Lines = []
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Order must have at least one line");

        _customers.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Lines =
            [
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            ]
        };

        _customers.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Customer not found");

        _customers.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenQuantityIsZeroOrNegative_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Lines =
            [
                new() { ProductId = Guid.NewGuid(), Quantity = 0 }
            ]
        };
        var customer = new Customer
        {
            Id = customerId,
            Name = "Jane Doe"
        };

        _customers.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Quantity must be greater than zero");

        _customers.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _products.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Lines =
            [
                new() { ProductId = productId, Quantity = 1 }
            ]
        };
        var customer = new Customer
        {
            Id = customerId,
            Name = "Jane Doe"
        };

        _customers.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _products.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("One or more products not found");

        _customers.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _products.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
