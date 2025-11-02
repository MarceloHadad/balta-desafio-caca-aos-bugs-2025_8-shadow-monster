using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Data.Repositories;
using BugStore.Infrastructure.Tests.Data.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Tests.Data.Repositories;

public class OrderLineRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleOrderLinesToContext()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderLineRepository(context);
        var orderLines = new[]
        {
            new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 2,
                Total = 100m
            },
            new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 3,
                Total = 150m
            }
        };

        // Act
        await repository.AddRangeAsync(orderLines);

        // Assert
        foreach (var line in orderLines)
        {
            var entry = context.Entry(line);
            entry.State.Should().Be(EntityState.Added);
        }
    }

    [Fact]
    public async Task AddRangeAsync_WhenEmptyCollection_ShouldNotAddAny()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderLineRepository(context);
        var emptyList = Array.Empty<OrderLine>();

        // Act
        await repository.AddRangeAsync(emptyList);

        // Assert
        context.ChangeTracker.Entries<OrderLine>().Should().BeEmpty();
    }

    [Fact]
    public void DeleteRange_ShouldMarkMultipleOrderLinesAsDeleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderLineRepository(context);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@test.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };
        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 50m
        };
        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Product 2",
            Description = "Description 2",
            Slug = "product-2",
            Price = 75m
        };
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Customers.Add(customer);
        context.Products.AddRange(product1, product2);
        context.Orders.Add(order);
        context.SaveChanges();

        var orderLines = new[]
        {
            new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product1.Id,
                Quantity = 1,
                Total = 50m
            },
            new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product2.Id,
                Quantity = 2,
                Total = 75m
            }
        };
        context.OrderLines.AddRange(orderLines);
        context.SaveChanges();

        // Act
        repository.DeleteRange(orderLines);

        // Assert
        foreach (var line in orderLines)
        {
            var entry = context.Entry(line);
            entry.State.Should().Be(EntityState.Deleted);
        }
    }

    [Fact]
    public void DeleteRange_WhenEmptyCollection_ShouldNotDeleteAny()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderLineRepository(context);
        var emptyList = Array.Empty<OrderLine>();

        // Act
        repository.DeleteRange(emptyList);

        // Assert
        context.ChangeTracker.Entries<OrderLine>()
            .Where(e => e.State == EntityState.Deleted)
            .Should().BeEmpty();
    }
}
