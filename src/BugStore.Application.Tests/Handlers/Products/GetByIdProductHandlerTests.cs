using BugStore.Application.Handlers.Products;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Products;

public class GetByIdProductHandlerTests
{
    private readonly GetByIdProductHandler _handler;
    private readonly Mock<IProductRepository> _repo;

    public GetByIdProductHandlerTests()
    {
        _repo = new Mock<IProductRepository>();
        _handler = new GetByIdProductHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProductExists_ReturnsResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new GetByIdProductRequest(productId);
        var product = new Product
        {
            Id = productId,
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };

        _repo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(product.Id);
        response.Title.Should().Be(product.Title);
        response.Description.Should().Be(product.Description);
        response.Slug.Should().Be(product.Slug);
        response.Price.Should().Be(product.Price);

        _repo.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new GetByIdProductRequest(productId);

        _repo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Product not found");

        _repo.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }
}
