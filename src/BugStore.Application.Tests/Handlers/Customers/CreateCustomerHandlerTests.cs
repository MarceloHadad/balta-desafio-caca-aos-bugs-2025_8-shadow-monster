using BugStore.Application.Handlers.Customers;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Customers;

public class CreateCustomerHandlerTests
{
    private readonly CreateCustomerHandler _handler;
    private readonly Mock<ICustomerRepository> _repo;
    private readonly Mock<IUnitOfWork> _uow;

    public CreateCustomerHandlerTests()
    {
        _repo = new Mock<ICustomerRepository>();
        _uow = new Mock<IUnitOfWork>();
        _handler = new CreateCustomerHandler(_repo.Object, _uow.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidRequest_PersistsAndReturnsResponse()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        _repo.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((Customer?)null);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.Phone.Should().Be(request.Phone);
        response.BirthDate.Should().Be(request.BirthDate);

        _repo.Verify(r => r.GetByEmailAsync(request.Email), Times.Once);
        _repo.Verify(r => r.AddAsync(It.Is<Customer>(c =>
            c.Name == request.Name &&
            c.Email == request.Email &&
            c.Phone == request.Phone &&
            c.BirthDate == request.BirthDate
        )), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
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

        _repo.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
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

        _repo.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPhoneIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
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

        _repo.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenBirthDateIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
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

        _repo.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenBirthDateInFuture_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
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

        _repo.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailInvalid_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
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

        _repo.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = request.Email,
            Phone = "+55 11 88888-0000",
            BirthDate = new DateTime(1985, 5, 5)
        };

        _repo.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingCustomer);

        // Act
        var act = async () => await _handler.HandleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        ex.Message.Should().Be("Email already in use");

        _repo.Verify(r => r.GetByEmailAsync(request.Email), Times.Once);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
