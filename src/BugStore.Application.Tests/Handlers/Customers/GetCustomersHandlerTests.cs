using BugStore.Application.Handlers.Customers;
using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Customers.Search;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Customers;

public class GetCustomersHandlerTests
{
    private readonly GetCustomersHandler _handler;
    private readonly Mock<ICustomerRepository> _repo;

    public GetCustomersHandlerTests()
    {
        _repo = new Mock<ICustomerRepository>();
        _handler = new GetCustomersHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomersExist_ReturnsAllCustomers()
    {
        // Arrange
        var request = new SearchCustomersRequest();
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Phone = "+55 11 99999-0000",
                BirthDate = new DateTime(1990, 1, 1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "+55 11 88888-0000",
                BirthDate = new DateTime(1985, 5, 5)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Customer>)customers, customers.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().HaveCount(2);
        response.Customers[0].Id.Should().Be(customers[0].Id);
        response.Customers[0].Name.Should().Be(customers[0].Name);
        response.Customers[1].Id.Should().Be(customers[1].Id);
        response.Customers[1].Name.Should().Be(customers[1].Name);

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoCustomers_ReturnsEmptyList()
    {
        // Arrange
        var request = new SearchCustomersRequest();

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Customer>)new List<Customer>(), 0));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().BeEmpty();

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByName_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchCustomersRequest { Name = "Jane" };
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Phone = "+55 11 99999-0000",
                BirthDate = new DateTime(1990, 1, 1)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Customer>)customers, customers.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().HaveCount(1);
        response.Customers[0].Name.Should().Be("Jane Doe");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchCustomersRequest>(req => req.Name == "Jane")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByEmail_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchCustomersRequest { Email = "john@" };
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "+55 11 88888-0000",
                BirthDate = new DateTime(1985, 5, 5)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Customer>)customers, customers.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().HaveCount(1);
        response.Customers[0].Email.Should().Contain("john");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchCustomersRequest>(req => req.Email == "john@")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilterByPhone_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchCustomersRequest { Phone = "99999" };
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Phone = "+55 11 99999-0000",
                BirthDate = new DateTime(1990, 1, 1)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Customer>)customers, customers.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().HaveCount(1);
        response.Customers[0].Phone.Should().Contain("99999");

        _repo.Verify(r => r.SearchAsync(It.Is<SearchCustomersRequest>(req => req.Phone == "99999")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleFilters_CallsSearchAsync()
    {
        // Arrange
        var request = new SearchCustomersRequest
        {
            Name = "Jane",
            Email = "jane@",
            Phone = "99999"
        };
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Phone = "+55 11 99999-0000",
                BirthDate = new DateTime(1990, 1, 1)
            }
        };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Customer>)customers, customers.Count));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().HaveCount(1);

        _repo.Verify(r => r.SearchAsync(It.Is<SearchCustomersRequest>(req =>
            req.Name == "Jane" && req.Email == "jane@" && req.Phone == "99999")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSearchReturnsNoResults_ReturnsEmptyList()
    {
        // Arrange
        var request = new SearchCustomersRequest { Name = "NonExistent" };

        _repo.Setup(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()))
            .ReturnsAsync(((IReadOnlyList<Customer>)new List<Customer>(), 0));

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().BeEmpty();

        _repo.Verify(r => r.SearchAsync(It.IsAny<SearchCustomersRequest>()), Times.Once);
    }
}

