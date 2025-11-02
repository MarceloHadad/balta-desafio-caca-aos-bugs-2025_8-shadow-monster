using BugStore.Application.Handlers.Products;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Products;

public class CreateProductHandlerTests
{
    private readonly CreateProductHandler _handler;
    private readonly Mock<IProductRepository> _repo;
    private readonly Mock<IUnitOfWork> _uow;

    public CreateProductHandlerTests()
    {
        _repo = new Mock<IProductRepository>();
        _uow = new Mock<IUnitOfWork>();
        _handler = new CreateProductHandler(_repo.Object, _uow.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidRequest_PersistsAndReturnsResponse()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };

        _repo.Setup(r => r.GetBySlugAsync(request.Slug))
            .ReturnsAsync((Product?)null);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Title.Should().Be(request.Title);
        response.Description.Should().Be(request.Description);
        response.Slug.Should().Be(request.Slug);
        response.Price.Should().Be(request.Price);

        _repo.Verify(r => r.GetBySlugAsync(request.Slug), Times.Once);
        _repo.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Title == request.Title &&
            p.Description == request.Description &&
            p.Slug == request.Slug &&
            p.Price == request.Price
        )), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Title is required");

        _repo.Verify(r => r.GetBySlugAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDescriptionIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Title = "Product 1",
            Slug = "product-1",
            Price = 100.00m
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Description is required");

        _repo.Verify(r => r.GetBySlugAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSlugIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Title = "Product 1",
            Description = "Description 1",
            Price = 100.00m
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Slug is required");

        _repo.Verify(r => r.GetBySlugAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPriceIsZeroOrNegative_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 0
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Price must be greater than zero");

        _repo.Verify(r => r.GetBySlugAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSlugAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 100.00m
        };
        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Existing Product",
            Slug = request.Slug
        };

        _repo.Setup(r => r.GetBySlugAsync(request.Slug))
            .ReturnsAsync(existingProduct);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        ex.Message.Should().Be("Slug already in use");

        _repo.Verify(r => r.GetBySlugAsync(request.Slug), Times.Once);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
