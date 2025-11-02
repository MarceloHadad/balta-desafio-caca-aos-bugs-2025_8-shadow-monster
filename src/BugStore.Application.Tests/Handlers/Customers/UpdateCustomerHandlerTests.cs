using BugStore.Application.Handlers.Customers;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Customers;

public class UpdateCustomerHandlerTests
{
    private readonly UpdateCustomerHandler _handler;
    private readonly Mock<ICustomerRepository> _repo;
    private readonly Mock<IUnitOfWork> _uow;

    public UpdateCustomerHandlerTests()
    {
        _repo = new Mock<ICustomerRepository>();
        _uow = new Mock<IUnitOfWork>();
        _handler = new UpdateCustomerHandler(_repo.Object, _uow.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidRequest_UpdatesAndReturnsResponse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Jane Updated",
            Email = "jane.updated@example.com",
            Phone = "+55 11 88888-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };
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
        _repo.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((Customer?)null);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(customerId);
        response.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.Phone.Should().Be(request.Phone);
        response.BirthDate.Should().Be(request.BirthDate);

        _repo.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _repo.Verify(r => r.GetByEmailAsync(request.Email), Times.Once);
        _repo.Verify(r => r.UpdateAsync(existingCustomer), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateCustomerRequest(Guid.NewGuid())
        {
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Name is required");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateCustomerRequest(Guid.NewGuid())
        {
            Name = "Jane Doe",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Email is required");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPhoneIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateCustomerRequest(Guid.NewGuid())
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Phone is required");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenBirthDateIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateCustomerRequest(Guid.NewGuid())
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("BirthDate is required");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenBirthDateInFuture_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateCustomerRequest(Guid.NewGuid())
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = DateTime.UtcNow.Date.AddDays(1)
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("BirthDate cannot be in the future");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailInvalid_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateCustomerRequest(Guid.NewGuid())
        {
            Name = "Jane Doe",
            Email = "invalid-email",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        ex.Message.Should().Be("Email is invalid");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        _repo.Setup(r => r.GetByIdAsync(request.Id))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        ex.Message.Should().Be("Customer not found");

        _repo.Verify(r => r.GetByIdAsync(request.Id), Times.Once);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Jane Doe",
            Email = "taken@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };
        var existingCustomer = new Customer
        {
            Id = customerId,
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };
        var emailOwner = new Customer
        {
            Id = otherCustomerId,
            Email = "taken@example.com"
        };

        _repo.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);
        _repo.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(emailOwner);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        ex.Message.Should().Be("Email already in use");

        _repo.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _repo.Verify(r => r.GetByEmailAsync(request.Email), Times.Once);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
