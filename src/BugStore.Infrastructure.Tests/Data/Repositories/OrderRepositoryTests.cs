using BugStore.Application.UseCases.Orders.Search;
using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Data.Repositories;
using BugStore.Infrastructure.Tests.Data.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Tests.Data.Repositories;

public class OrderRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task AddAsync_ShouldAddOrderToContext()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "+1 555-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(order);

        // Assert
        var entry = context.Entry(order);
        entry.State.Should().Be(EntityState.Added);
        entry.Entity.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenOrderExists_ReturnsOrderWithDetails()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1 555-1234",
            BirthDate = new DateTime(1985, 5, 15)
        };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Test Product",
            Description = "Description",
            Slug = "test-product",
            Price = 50m
        };
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        var orderLine = new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = 2,
            Total = 100m
        };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.Add(order);
        context.OrderLines.Add(orderLine);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdWithDetailsAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Customer.Should().NotBeNull();
        result.Customer.Id.Should().Be(customer.Id);
        result.Lines.Should().HaveCount(1);
        result.Lines.First().Product.Should().NotBeNull();
        result.Lines.First().Product.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdWithDetailsAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WhenNoFilters_ReturnsAllOrdersWithDetails()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer1 = new Customer { Id = Guid.NewGuid(), Name = "Customer 1", Email = "c1@example.com", Phone = "+1 555-0001", BirthDate = DateTime.UtcNow };
        var customer2 = new Customer { Id = Guid.NewGuid(), Name = "Customer 2", Email = "c2@example.com", Phone = "+1 555-0002", BirthDate = DateTime.UtcNow };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 10m };

        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer1.Id, CreatedAt = DateTime.UtcNow };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer2.Id, CreatedAt = DateTime.UtcNow };

        context.Customers.AddRange(customer1, customer2);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.SearchAsync(new SearchOrdersRequest());

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(o => o.Id == order1.Id);
        result.Items.Should().Contain(o => o.Id == order2.Id);
        result.Items.All(o => o.Customer != null).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WhenNoFiltersAndNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.SearchAsync(new SearchOrdersRequest());

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_ShouldMarkOrderAsModified()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@test.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var trackedOrder = context.Orders.Find(order.Id);

        // Act
        await repository.Update(trackedOrder!);

        // Assert
        var entry = context.Entry(trackedOrder!);
        entry.State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task Update_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var nonExistentOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var act = async () => await repository.Update(nonExistentOrder);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Order not found");
    }

    [Fact]
    public async Task Delete_ShouldMarkOrderAsDeleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@test.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Act
        await repository.Delete(order);

        // Assert
        var entry = context.Entry(order);
        entry.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task Delete_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var nonExistentOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var act = async () => await repository.Delete(nonExistentOrder);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Order not found");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterById_ReturnsMatchingOrder()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 100m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            Id = order1.Id
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Id.Should().Be(order1.Id);
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByCustomerName_ReturnsMatchingOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer1 = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var customer2 = new Customer { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com", Phone = "+1 555-5678", BirthDate = new DateTime(1990, 8, 20) };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 100m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer1.Id, CreatedAt = DateTime.UtcNow };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer2.Id, CreatedAt = DateTime.UtcNow };

        context.Customers.AddRange(customer1, customer2);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            CustomerName = "john"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Customer.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByCustomerEmail_ReturnsMatchingOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer1 = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var customer2 = new Customer { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@test.com", Phone = "+1 555-5678", BirthDate = new DateTime(1990, 8, 20) };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 100m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer1.Id, CreatedAt = DateTime.UtcNow };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer2.Id, CreatedAt = DateTime.UtcNow };

        context.Customers.AddRange(customer1, customer2);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            CustomerEmail = "example"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Customer.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByProductTitle_ReturnsMatchingOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var product1 = new Product { Id = Guid.NewGuid(), Title = "Gaming Laptop", Description = "Desc", Slug = "gaming-laptop", Price = 2500m };
        var product2 = new Product { Id = Guid.NewGuid(), Title = "Office Mouse", Description = "Desc", Slug = "office-mouse", Price = 50m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };
        var line1 = new OrderLine { Id = Guid.NewGuid(), OrderId = order1.Id, ProductId = product1.Id, Quantity = 1, Total = 2500m };
        var line2 = new OrderLine { Id = Guid.NewGuid(), OrderId = order2.Id, ProductId = product2.Id, Quantity = 1, Total = 50m };

        context.Customers.Add(customer);
        context.Products.AddRange(product1, product2);
        context.Orders.AddRange(order1, order2);
        context.OrderLines.AddRange(line1, line2);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            ProductTitle = "laptop"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Lines.First().Product.Title.Should().Contain("Laptop");
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByProductPriceRange_ReturnsMatchingOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var product1 = new Product { Id = Guid.NewGuid(), Title = "Expensive Product", Description = "Desc", Slug = "expensive", Price = 2500m };
        var product2 = new Product { Id = Guid.NewGuid(), Title = "Cheap Product", Description = "Desc", Slug = "cheap", Price = 50m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };
        var line1 = new OrderLine { Id = Guid.NewGuid(), OrderId = order1.Id, ProductId = product1.Id, Quantity = 1, Total = 2500m };
        var line2 = new OrderLine { Id = Guid.NewGuid(), OrderId = order2.Id, ProductId = product2.Id, Quantity = 1, Total = 50m };

        context.Customers.Add(customer);
        context.Products.AddRange(product1, product2);
        context.Orders.AddRange(order1, order2);
        context.OrderLines.AddRange(line1, line2);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            ProductPriceStart = 1000m,
            ProductPriceEnd = 3000m
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Lines.First().Product.Price.Should().Be(2500m);
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByCreatedAtRange_ReturnsMatchingOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 100m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 1, 15), UpdatedAt = new DateTime(2025, 1, 15) };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 6, 15), UpdatedAt = new DateTime(2025, 6, 15) };
        var order3 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 12, 15), UpdatedAt = new DateTime(2025, 12, 15) };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2, order3);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            CreatedAtStart = new DateTime(2025, 3, 1),
            CreatedAtEnd = new DateTime(2025, 9, 1)
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().CreatedAt.Should().Be(new DateTime(2025, 6, 15));
    }

    [Fact]
    public async Task SearchAsync_WhenFilterByUpdatedAtRange_ReturnsMatchingOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 100m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 1, 1), UpdatedAt = new DateTime(2025, 1, 15) };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 1, 1), UpdatedAt = new DateTime(2025, 6, 15) };
        var order3 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 1, 1), UpdatedAt = new DateTime(2025, 12, 15) };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2, order3);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            UpdatedAtStart = new DateTime(2025, 3, 1),
            UpdatedAtEnd = new DateTime(2025, 9, 1)
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().UpdatedAt.Should().Be(new DateTime(2025, 6, 15));
    }

    [Fact]
    public async Task SearchAsync_WhenMultipleFilters_ReturnsMatchingOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer1 = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var customer2 = new Customer { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com", Phone = "+1 555-5678", BirthDate = new DateTime(1990, 8, 20) };
        var product1 = new Product { Id = Guid.NewGuid(), Title = "Gaming Laptop", Description = "High-end", Slug = "gaming-laptop", Price = 2500m };
        var product2 = new Product { Id = Guid.NewGuid(), Title = "Office Mouse", Description = "Standard", Slug = "office-mouse", Price = 50m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer1.Id, CreatedAt = new DateTime(2025, 6, 1), UpdatedAt = new DateTime(2025, 6, 1) };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer2.Id, CreatedAt = new DateTime(2025, 6, 1), UpdatedAt = new DateTime(2025, 6, 1) };
        var line1 = new OrderLine { Id = Guid.NewGuid(), OrderId = order1.Id, ProductId = product1.Id, Quantity = 1, Total = 2500m };
        var line2 = new OrderLine { Id = Guid.NewGuid(), OrderId = order2.Id, ProductId = product2.Id, Quantity = 1, Total = 50m };

        context.Customers.AddRange(customer1, customer2);
        context.Products.AddRange(product1, product2);
        context.Orders.AddRange(order1, order2);
        context.OrderLines.AddRange(line1, line2);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            CustomerName = "john",
            ProductTitle = "laptop",
            ProductPriceStart = 1000m
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Customer.Name.Should().Be("John Doe");
        result.Items.First().Lines.First().Product.Title.Should().Contain("Laptop");
    }

    [Fact]
    public async Task SearchAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 100m };
        var order = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = DateTime.UtcNow };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            CustomerName = "NonExistent"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ReturnsOrderedByCreatedAtDescending()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Phone = "+1 555-1234", BirthDate = new DateTime(1985, 5, 15) };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 100m };
        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 1, 1), UpdatedAt = new DateTime(2025, 1, 1) };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 6, 1), UpdatedAt = new DateTime(2025, 6, 1) };
        var order3 = new Order { Id = Guid.NewGuid(), CustomerId = customer.Id, CreatedAt = new DateTime(2025, 3, 1), UpdatedAt = new DateTime(2025, 3, 1) };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2, order3);
        await context.SaveChangesAsync();

        var request = new BugStore.Application.UseCases.Orders.Search.SearchOrdersRequest
        {
            CustomerName = "john"
        };

        // Act
        var result = await repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].CreatedAt.Should().Be(new DateTime(2025, 6, 1));
        result.Items[1].CreatedAt.Should().Be(new DateTime(2025, 3, 1));
        result.Items[2].CreatedAt.Should().Be(new DateTime(2025, 1, 1));
    }
}
