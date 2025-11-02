using BugStore.Application.Handlers.Reports;
using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Reports.BestCustomers;
using FluentAssertions;
using Moq;

namespace BugStore.Application.Tests.Handlers.Reports;

public class BestCustomersHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnAllCustomers_WhenNoFiltersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest();
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m },
            new() { CustomerName = "Jane Smith", CustomerEmail = "jane@example.com", TotalOrders = 3, SpentAmount = 300m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().NotBeNull();
        result.Customers.Should().HaveCount(2);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoCustomersFound()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { CustomerName = "NonExistent" };
        var expectedData = new List<BestCustomersResponse>();

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().NotBeNull();
        result.Customers.Should().BeEmpty();
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByName_WhenNameProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { CustomerName = "John" };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByEmail_WhenEmailProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { CustomerEmail = "john@example.com" };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMinOrders_WhenMinOrdersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { MinOrders = 5 };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMaxOrders_WhenMaxOrdersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { MaxOrders = 3 };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "Jane Smith", CustomerEmail = "jane@example.com", TotalOrders = 3, SpentAmount = 300m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMinSpent_WhenMinSpentProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { MinSpent = 500m };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMaxSpent_WhenMaxSpentProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { MaxSpent = 300m };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "Jane Smith", CustomerEmail = "jane@example.com", TotalOrders = 3, SpentAmount = 300m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyMultipleFilters_WhenMultipleFiltersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest
        {
            CustomerName = "John",
            MinOrders = 5,
            MinSpent = 500m
        };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldOrderByTotalOrdersAscending_WhenSortByTotalOrdersAsc()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { OrderBy = "totalOrders", OrderDirection = "asc" };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "Jane Smith", CustomerEmail = "jane@example.com", TotalOrders = 3, SpentAmount = 300m },
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(2);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldOrderBySpentAmountDescending_WhenSortBySpentAmountDesc()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new BestCustomersHandler(mockRepository.Object);

        var request = new BestCustomersRequest { OrderBy = "spentAmount", OrderDirection = "desc" };
        var expectedData = new List<BestCustomersResponse>
        {
            new() { CustomerName = "John Doe", CustomerEmail = "john@example.com", TotalOrders = 5, SpentAmount = 500m },
            new() { CustomerName = "Jane Smith", CustomerEmail = "jane@example.com", TotalOrders = 3, SpentAmount = 300m }
        };

        mockRepository
            .Setup(repo => repo.GetBestCustomersAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(2);
        result.Customers.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetBestCustomersAsync(request), Times.Once);
    }
}
