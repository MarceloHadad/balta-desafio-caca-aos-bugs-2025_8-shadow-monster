using BugStore.Application.Handlers.Products;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Products;

public class UpdateProductHandlerTests
{
    private readonly UpdateProductHandler _handler;
    private readonly Mock<IProductRepository> _repo;
    private readonly Mock<IUnitOfWork> _uow;

    public UpdateProductHandlerTests()
    {
        _repo = new Mock<IProductRepository>();
        _uow = new Mock<IUnitOfWork>();
        _handler = new UpdateProductHandler(_repo.Object, _uow.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidRequest_UpdatesAndReturnsResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new UpdateProductRequest
        {
            Id = productId,
            Title = "Updated Product",
            Description = "Updated Description",
            Slug = "updated-product",
            Price = 150.00m
        };
        var existingProduct = new Product
        {
            Id = productId,
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };

        _repo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _repo.Setup(r => r.GetBySlugAsync(request.Slug))
            .ReturnsAsync((Product?)null);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(productId);
        response.Title.Should().Be(request.Title);
        response.Description.Should().Be(request.Description);
        response.Slug.Should().Be(request.Slug);
        response.Price.Should().Be(request.Price);

        _repo.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _repo.Verify(r => r.GetBySlugAsync(request.Slug), Times.Once);
        _repo.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Title is required");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDescriptionIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Title = "Product 1",
            Slug = "product-1",
            Price = 100.00m
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Description is required");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSlugIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Title = "Product 1",
            Description = "Description 1",
            Price = 100.00m
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Slug is required");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPriceIsZeroOrNegative_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = -10.00m
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Price must be greater than zero");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };

        _repo.Setup(r => r.GetByIdAsync(request.Id))
            .ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Product not found");

        _repo.Verify(r => r.GetByIdAsync(request.Id), Times.Once);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSlugAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        var request = new UpdateProductRequest
        {
            Id = productId,
            Title = "Product 1",
            Description = "Description 1",
            Slug = "taken-slug",
            Price = 100.00m
        };
        var existingProduct = new Product
        {
            Id = productId,
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };
        var slugOwner = new Product
        {
            Id = otherProductId,
            Slug = "taken-slug"
        };

        _repo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _repo.Setup(r => r.GetBySlugAsync(request.Slug))
            .ReturnsAsync(slugOwner);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        ex.Message.Should().Be("Slug already in use");

        _repo.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _repo.Verify(r => r.GetBySlugAsync(request.Slug), Times.Once);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
