using BugStore.Application.Handlers.Products;
using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Products.Search;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Products;

public class GetProductsHandlerTests
{
    private readonly GetProductsHandler _handler;
    private readonly Mock<IProductRepository> _repo;

    public GetProductsHandlerTests()
    {
        _repo = new Mock<IProductRepository>();
        _handler = new GetProductsHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProductsExist_ReturnsAllProducts()
    {
        // Arrange
        var request = new SearchProductsRequest();
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Product 1",
                Description = "Description 1",
                Slug = "product-1",
                Price = 100.00m
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Product 2",
                Description = "Description 2",
                Slug = "product-2",
                Price = 200.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)products, products.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(2);
        response.Products[0].Id.Should().Be(products[0].Id);
        response.Products[0].Title.Should().Be(products[0].Title);
        response.Products[1].Id.Should().Be(products[1].Id);
        response.Products[1].Title.Should().Be(products[1].Title);

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoProducts_ReturnsEmptyList()
    {
        // Arrange
        var request = new SearchProductsRequest();

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)new List<Product>(), 0));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().BeEmpty();

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByTitle_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchProductsRequest { Title = "Laptop" };
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Gaming Laptop",
                Description = "High-end gaming laptop",
                Slug = "gaming-laptop",
                Price = 2500.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)products, products.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(1);
        response.Products[0].Title.Should().Contain("Laptop");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchProductsRequest>(req => req.Title == "Laptop")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByDescription_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchProductsRequest { Description = "gaming" };
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Gaming Mouse",
                Description = "Professional gaming mouse",
                Slug = "gaming-mouse",
                Price = 150.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)products, products.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(1);
        response.Products[0].Description.Should().Contain("gaming");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchProductsRequest>(req => req.Description == "gaming")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterBySlug_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchProductsRequest { Slug = "laptop" };
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Gaming Laptop",
                Description = "High-end gaming laptop",
                Slug = "gaming-laptop",
                Price = 2500.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)products, products.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(1);
        response.Products[0].Slug.Should().Contain("laptop");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchProductsRequest>(req => req.Slug == "laptop")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByMinPrice_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchProductsRequest { MinPrice = 100.00m };
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Gaming Mouse",
                Description = "Professional gaming mouse",
                Slug = "gaming-mouse",
                Price = 150.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)products, products.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(1);
        response.Products[0].Price.Should().BeGreaterThanOrEqualTo(100.00m);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchProductsRequest>(req => req.MinPrice == 100.00m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByMaxPrice_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchProductsRequest { MaxPrice = 500.00m };
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Gaming Mouse",
                Description = "Professional gaming mouse",
                Slug = "gaming-mouse",
                Price = 150.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)products, products.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(1);
        response.Products[0].Price.Should().BeLessThanOrEqualTo(500.00m);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchProductsRequest>(req => req.MaxPrice == 500.00m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByPriceRange_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchProductsRequest { MinPrice = 100.00m, MaxPrice = 500.00m };
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Gaming Mouse",
                Description = "Professional gaming mouse",
                Slug = "gaming-mouse",
                Price = 150.00m
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard",
                Slug = "mechanical-keyboard",
                Price = 300.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .Returns(Task.FromResult(((IReadOnlyList<Product>)products, products.Count)));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(2);
        response.Products.Should().AllSatisfy(p => p.Price.Should().BeInRange(100.00m, 500.00m));

        _repo.Verify(r => r.SearchAsync(It.Is<SearchProductsRequest>(req =>
            req.MinPrice == 100.00m && req.MaxPrice == 500.00m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleFilters_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchProductsRequest
        {
            Title = "Gaming",
            Description = "professional",
            MinPrice = 100.00m,
            MaxPrice = 500.00m
        };
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Gaming Mouse",
                Description = "Professional gaming mouse",
                Slug = "gaming-mouse",
                Price = 150.00m
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .Returns(Task.FromResult(((IReadOnlyList<Product>)products, products.Count)));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(1);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchProductsRequest>(req =>
            req.Title == "Gaming" &&
            req.Description == "professional" &&
            req.MinPrice == 100.00m &&
            req.MaxPrice == 500.00m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSearchReturnsNoResults_ReturnsEmptyList()
    {
        // Arrange
        var request = new SearchProductsRequest { Title = "NonExistent" };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()))
            .ReturnsAsync(((IReadOnlyList<Product>)new List<Product>(), 0));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().BeEmpty();

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchProductsRequest>()), Times.Once);
    }
}

