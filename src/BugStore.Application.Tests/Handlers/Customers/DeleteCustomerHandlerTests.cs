using BugStore.Application.Handlers.Customers;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Customers;

public class DeleteCustomerHandlerTests
{
    private readonly DeleteCustomerHandler _handler;
    private readonly Mock<ICustomerRepository> _repo;
    private readonly Mock<IUnitOfWork> _uow;

    public DeleteCustomerHandlerTests()
    {
        _repo = new Mock<ICustomerRepository>();
        _uow = new Mock<IUnitOfWork>();
        _handler = new DeleteCustomerHandler(_repo.Object, _uow.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidRequest_DeletesAndReturnsResponse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new DeleteCustomerRequest { Id = customerId };
        var existingCustomer = new Customer
        {
            Id = customerId,
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        _repo.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();

        _repo.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _repo.Verify(r => r.DeleteAsync(customerId), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new DeleteCustomerRequest { Id = customerId };

        _repo.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Customer not found");

        _repo.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
