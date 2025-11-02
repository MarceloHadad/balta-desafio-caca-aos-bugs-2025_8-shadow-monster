using BugStore.Application.Handlers.Products;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Products;

public class DeleteProductHandlerTests
{
    private readonly DeleteProductHandler _handler;
    private readonly Mock<IProductRepository> _repo;
    private readonly Mock<IUnitOfWork> _uow;

    public DeleteProductHandlerTests()
    {
        _repo = new Mock<IProductRepository>();
        _uow = new Mock<IUnitOfWork>();
        _handler = new DeleteProductHandler(_repo.Object, _uow.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidRequest_DeletesAndReturnsResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new DeleteProductRequest { Id = productId };
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

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();

        _repo.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _repo.Verify(r => r.DeleteAsync(productId), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new DeleteProductRequest { Id = productId };

        _repo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Product not found");

        _repo.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
