using BugStore.Application.Handlers.Orders;
using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Orders.Search;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Orders;

public class GetOrdersHandlerTests
{
    private readonly GetOrdersHandler _handler;
    private readonly Mock<IOrderRepository> _repo;

    public GetOrdersHandlerTests()
    {
        _repo = new Mock<IOrderRepository>();
        _handler = new GetOrdersHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenOrdersExist_ReturnsAllOrders()
    {
        // Arrange
        var request = new SearchOrdersRequest();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Product 1",
                            Description = "Description 1",
                            Slug = "product-1",
                            Price = 100.00m
                        },
                        Quantity = 2,
                        Total = 200.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].Id.Should().Be(orders[0].Id);
        response.Orders[0].CustomerName.Should().Be("John Doe");
        response.Orders[0].TotalAmount.Should().Be(200.00m);
        response.Orders[0].Lines.Should().HaveCount(1);

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var request = new SearchOrdersRequest();

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)new List<Order>(), 0));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().BeEmpty();

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterById_CallsSearchAsync()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new SearchOrdersRequest { Id = orderId };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = orderId,
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Product 1",
                            Description = "Description 1",
                            Slug = "product-1",
                            Price = 100.00m
                        },
                        Quantity = 2,
                        Total = 200.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].Id.Should().Be(orderId);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req => req.Id == orderId)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByCustomerName_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchOrdersRequest { CustomerName = "John" };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Product 1",
                            Description = "Description 1",
                            Slug = "product-1",
                            Price = 100.00m
                        },
                        Quantity = 1,
                        Total = 100.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].CustomerName.Should().Contain("John");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req => req.CustomerName == "John")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByCustomerEmail_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchOrdersRequest { CustomerEmail = "john@" };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Product 1",
                            Description = "Description 1",
                            Slug = "product-1",
                            Price = 100.00m
                        },
                        Quantity = 1,
                        Total = 100.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req => req.CustomerEmail == "john@")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByProductTitle_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchOrdersRequest { ProductTitle = "Laptop" };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Gaming Laptop",
                            Description = "High-end laptop",
                            Slug = "gaming-laptop",
                            Price = 2500.00m
                        },
                        Quantity = 1,
                        Total = 2500.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].Lines[0].ProductTitle.Should().Contain("Laptop");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req => req.ProductTitle == "Laptop")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByProductDescription_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchOrdersRequest { ProductDescription = "High-end" };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Gaming Laptop",
                            Description = "High-end laptop",
                            Slug = "gaming-laptop",
                            Price = 2500.00m
                        },
                        Quantity = 1,
                        Total = 2500.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req => req.ProductDescription == "High-end")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByProductPriceRange_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchOrdersRequest { ProductPriceStart = 100.00m, ProductPriceEnd = 500.00m };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Gaming Mouse",
                            Description = "Professional mouse",
                            Slug = "gaming-mouse",
                            Price = 150.00m
                        },
                        Quantity = 2,
                        Total = 300.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].Lines[0].UnitPrice.Should().BeInRange(100.00m, 500.00m);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req =>
            req.ProductPriceStart == 100.00m && req.ProductPriceEnd == 500.00m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByCreatedAtRange_CallsSearchAsync()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 12, 31);
        var request = new SearchOrdersRequest { CreatedAtStart = startDate, CreatedAtEnd = endDate };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Product 1",
                            Description = "Description 1",
                            Slug = "product-1",
                            Price = 100.00m
                        },
                        Quantity = 1,
                        Total = 100.00m
                    }
                },
                CreatedAt = new DateTime(2025, 6, 15),
                UpdatedAt = new DateTime(2025, 6, 15)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .Returns(Task.FromResult(((IReadOnlyList<Order>)orders, orders.Count)));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].CreatedAt.Should().BeOnOrAfter(startDate).And.BeOnOrBefore(endDate);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req =>
            req.CreatedAtStart == startDate && req.CreatedAtEnd == endDate)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByUpdatedAtRange_CallsSearchAsync()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 12, 31);
        var request = new SearchOrdersRequest { UpdatedAtStart = startDate, UpdatedAtEnd = endDate };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Product 1",
                            Description = "Description 1",
                            Slug = "product-1",
                            Price = 100.00m
                        },
                        Quantity = 1,
                        Total = 100.00m
                    }
                },
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 6, 15)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .Returns(Task.FromResult(((IReadOnlyList<Order>)orders, orders.Count)));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].UpdatedAt.Should().BeOnOrAfter(startDate).And.BeOnOrBefore(endDate);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req =>
            req.UpdatedAtStart == startDate && req.UpdatedAtEnd == endDate)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleFilters_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchOrdersRequest
        {
            CustomerName = "John",
            ProductTitle = "Laptop",
            ProductPriceStart = 1000.00m,
            CreatedAtStart = new DateTime(2025, 1, 1)
        };
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Product = new Product
                        {
                            Id = productId,
                            Title = "Gaming Laptop",
                            Description = "High-end laptop",
                            Slug = "gaming-laptop",
                            Price = 2500.00m
                        },
                        Quantity = 1,
                        Total = 2500.00m
                    }
                },
                CreatedAt = new DateTime(2025, 6, 15),
                UpdatedAt = new DateTime(2025, 6, 15)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .Returns(Task.FromResult(((IReadOnlyList<Order>)orders, orders.Count)));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchOrdersRequest>(req =>
            req.CustomerName == "John" &&
            req.ProductTitle == "Laptop" &&
            req.ProductPriceStart == 1000.00m &&
            req.CreatedAtStart == new DateTime(2025, 1, 1))), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSearchReturnsNoResults_ReturnsEmptyList()
    {
        // Arrange
        var request = new SearchOrdersRequest { CustomerName = "NonExistent" };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)new List<Order>(), 0));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().BeEmpty();

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CalculatesTotalAmountCorrectly()
    {
        // Arrange
        var request = new SearchOrdersRequest();
        var customerId = Guid.NewGuid();
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "+55 11 99999-0000",
                    BirthDate = new DateTime(1990, 1, 1)
                },
                Lines = new List<OrderLine>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product1Id,
                        Product = new Product
                        {
                            Id = product1Id,
                            Title = "Product 1",
                            Description = "Description 1",
                            Slug = "product-1",
                            Price = 100.00m
                        },
                        Quantity = 2,
                        Total = 200.00m
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product2Id,
                        Product = new Product
                        {
                            Id = product2Id,
                            Title = "Product 2",
                            Description = "Description 2",
                            Slug = "product-2",
                            Price = 50.00m
                        },
                        Quantity = 3,
                        Total = 150.00m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchOrdersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Order>)orders, orders.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders[0].TotalAmount.Should().Be(350.00m);
        response.Orders[0].Lines.Should().HaveCount(2);
    }
}


