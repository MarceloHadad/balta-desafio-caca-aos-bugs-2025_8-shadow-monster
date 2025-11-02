using BugStore.Application.Handlers.Customers;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Customers;

public class GetByIdCustomerHandlerTests
{
    private readonly GetByIdCustomerHandler _handler;
    private readonly Mock<ICustomerRepository> _repo;

    public GetByIdCustomerHandlerTests()
    {
        _repo = new Mock<ICustomerRepository>();
        _handler = new GetByIdCustomerHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerExists_ReturnsResponse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new GetByIdCustomerRequest(customerId);
        var customer = new Customer
        {
            Id = customerId,
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        _repo.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(customer.Id);
        response.Name.Should().Be(customer.Name);
        response.Email.Should().Be(customer.Email);
        response.Phone.Should().Be(customer.Phone);
        response.BirthDate.Should().Be(customer.BirthDate);

        _repo.Verify(r => r.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new GetByIdCustomerRequest(customerId);

        _repo.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Customer not found");

        _repo.Verify(r => r.GetByIdAsync(customerId), Times.Once);
    }
}
