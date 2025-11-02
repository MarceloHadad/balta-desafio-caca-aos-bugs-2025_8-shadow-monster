using BugStore.Application.UseCases.Products.Search;
using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Data.Repositories;
using BugStore.Infrastructure.Tests.Data.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Tests.Data.Repositories;

public class ProductRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task AddAsync_ShouldAddProductToContext()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Test Product",
            Description = "Test Description",
            Slug = "test-product",
            Price = 99.99m
        };

        // Act
        await repository.AddAsync(product);

        // Assert
        var entry = context.Entry(product);
        entry.State.Should().Be(EntityState.Added);
        entry.Entity.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Existing Product",
            Description = "Some description",
            Slug = "existing-product",
            Price = 49.99m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Slugged Product",
            Description = "Product with slug",
            Slug = "unique-slug",
            Price = 29.99m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetBySlugAsync(product.Slug);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetBySlugAsync("nonexistent-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_WhenProductsExist_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product1 = new Product { Id = Guid.NewGuid(), Title = "Product 1", Description = "Desc 1", Slug = "product-1", Price = 10m };
        var product2 = new Product { Id = Guid.NewGuid(), Title = "Product 2", Description = "Desc 2", Slug = "product-2", Price = 20m };
        var product3 = new Product { Id = Guid.NewGuid(), Title = "Product 3", Description = "Desc 3", Slug = "product-3", Price = 30m };
        context.Products.AddRange(product1, product2, product3);
        await context.SaveChangesAsync();

        var idsToRetrieve = new[] { product1.Id, product3.Id };

        // Act
        var result = await repository.GetByIdsAsync(idsToRetrieve);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product3.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNoProductsMatch_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await repository.GetByIdsAsync(nonExistentIds);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WhenNoFilters_ReturnsAllOrderedByTitle()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Zebra Product", Description = "Desc", Slug = "zebra", Price = 10m },
            new Product { Id = Guid.NewGuid(), Title = "Alpha Product", Description = "Desc", Slug = "alpha", Price = 20m },
            new Product { Id = Guid.NewGuid(), Title = "Beta Product", Description = "Desc", Slug = "beta", Price = 30m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.SearchAsync(new SearchProductsRequest());

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Title.Should().Be("Alpha Product");
        result.Items[1].Title.Should().Be("Beta Product");
        result.Items[2].Title.Should().Be("Zebra Product");
    }

    [Fact]
    public async Task SearchAsync_WhenNoFiltersAndNoProducts_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.SearchAsync(new SearchProductsRequest());

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesTrackedEntity()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Description = "Original Description",
            Slug = "original-slug",
            Price = 100m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var updatedProduct = new Product
        {
            Id = product.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            Slug = "updated-slug",
            Price = 200m
        };

        // Act
        await repository.UpdateAsync(updatedProduct);

        // Assert
        var entry = context.Entry(context.Products.Local.First(p => p.Id == product.Id));
        entry.State.Should().Be(EntityState.Modified);
        entry.Entity.Title.Should().Be("Updated Title");
        entry.Entity.Description.Should().Be("Updated Description");
        entry.Entity.Slug.Should().Be("updated-slug");
        entry.Entity.Price.Should().Be(200m);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentProduct = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Ghost Product",
            Description = "Does not exist",
            Slug = "ghost",
            Price = 0m
        };

        // Act
        var act = async () => await repository.UpdateAsync(nonExistentProduct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_MarksAsDeleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "To Delete",
            Description = "Will be deleted",
            Slug = "to-delete",
            Price = 50m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(product.Id);

        // Assert
        var entry = context.Entry(product);
        entry.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await repository.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByTitle_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Gaming Laptop", Description = "High-end laptop", Slug = "gaming-laptop", Price = 2500m },
            new Product { Id = Guid.NewGuid(), Title = "Office Mouse", Description = "Standard mouse", Slug = "office-mouse", Price = 50m },
            new Product { Id = Guid.NewGuid(), Title = "Gaming Mouse", Description = "RGB mouse", Slug = "gaming-mouse", Price = 150m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            Title = "gaming"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Title == "Gaming Laptop");
        result.Items.Should().Contain(p => p.Title == "Gaming Mouse");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByDescription_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Product 1", Description = "High-end equipment", Slug = "product-1", Price = 1000m },
            new Product { Id = Guid.NewGuid(), Title = "Product 2", Description = "Budget friendly", Slug = "product-2", Price = 100m },
            new Product { Id = Guid.NewGuid(), Title = "Product 3", Description = "High-end performance", Slug = "product-3", Price = 2000m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            Description = "high-end"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Description == "High-end equipment");
        result.Items.Should().Contain(p => p.Description == "High-end performance");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterBySlug_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Product 1", Description = "Desc 1", Slug = "laptop-gaming", Price = 1000m },
            new Product { Id = Guid.NewGuid(), Title = "Product 2", Description = "Desc 2", Slug = "mouse-wireless", Price = 100m },
            new Product { Id = Guid.NewGuid(), Title = "Product 3", Description = "Desc 3", Slug = "laptop-office", Price = 800m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            Slug = "laptop"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Slug == "laptop-gaming");
        result.Items.Should().Contain(p => p.Slug == "laptop-office");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByMinPrice_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Cheap Product", Description = "Desc", Slug = "cheap", Price = 50m },
            new Product { Id = Guid.NewGuid(), Title = "Mid Product", Description = "Desc", Slug = "mid", Price = 500m },
            new Product { Id = Guid.NewGuid(), Title = "Expensive Product", Description = "Desc", Slug = "expensive", Price = 2000m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            MinPrice = 500m
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Price == 500m);
        result.Items.Should().Contain(p => p.Price == 2000m);
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByMaxPrice_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Cheap Product", Description = "Desc", Slug = "cheap", Price = 50m },
            new Product { Id = Guid.NewGuid(), Title = "Mid Product", Description = "Desc", Slug = "mid", Price = 500m },
            new Product { Id = Guid.NewGuid(), Title = "Expensive Product", Description = "Desc", Slug = "expensive", Price = 2000m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            MaxPrice = 500m
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Price == 50m);
        result.Items.Should().Contain(p => p.Price == 500m);
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByPriceRange_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Product 1", Description = "Desc", Slug = "p1", Price = 50m },
            new Product { Id = Guid.NewGuid(), Title = "Product 2", Description = "Desc", Slug = "p2", Price = 150m },
            new Product { Id = Guid.NewGuid(), Title = "Product 3", Description = "Desc", Slug = "p3", Price = 500m },
            new Product { Id = Guid.NewGuid(), Title = "Product 4", Description = "Desc", Slug = "p4", Price = 2000m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            MinPrice = 100m,
            MaxPrice = 1000m
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Price == 150m);
        result.Items.Should().Contain(p => p.Price == 500m);
    }

    [Fact]
    public async Task SearchAsync_WhenMultipleFilters_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Gaming Laptop", Description = "High-end gaming", Slug = "gaming-laptop", Price = 2500m },
            new Product { Id = Guid.NewGuid(), Title = "Gaming Mouse", Description = "High-end mouse", Slug = "gaming-mouse", Price = 150m },
            new Product { Id = Guid.NewGuid(), Title = "Office Laptop", Description = "Budget laptop", Slug = "office-laptop", Price = 800m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            Title = "gaming",
            Description = "high-end",
            MinPrice = 1000m
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Gaming Laptop");
    }

    [Fact]
    public async Task SearchAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Product 1", Description = "Desc", Slug = "p1", Price = 100m },
            new Product { Id = Guid.NewGuid(), Title = "Product 2", Description = "Desc", Slug = "p2", Price = 200m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            Title = "NonExistent"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ReturnsOrderedByTitle()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Zebra Product", Description = "Desc", Slug = "zebra", Price = 100m },
            new Product { Id = Guid.NewGuid(), Title = "Alpha Product", Description = "Desc", Slug = "alpha", Price = 200m },
            new Product { Id = Guid.NewGuid(), Title = "Beta Product", Description = "Desc", Slug = "beta", Price = 300m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Products.Search.SearchProductsRequest
        {
            Description = "desc"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Title.Should().Be("Alpha Product");
        result.Items[1].Title.Should().Be("Beta Product");
        result.Items[2].Title.Should().Be("Zebra Product");
    }
}
