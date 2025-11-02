using BugStore.Application.Handlers.Reports;
using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Reports.RevenueByPeriod;
using FluentAssertions;
using Moq;

namespace BugStore.Application.Tests.Handlers.Reports;

public class RevenueByPeriodHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnAllPeriods_WhenNoFiltersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest();
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "January", TotalOrders = 10, TotalRevenue = 1000m },
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoPeriodsFound()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { StartPeriod = "2099-01", EndPeriod = "2099-12" };
        var expectedData = new List<RevenueByPeriodResponse>();

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByStartPeriod_WhenStartPeriodProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { StartPeriod = "2024-02" };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m },
            new() { Year = 2024, Month = "March", TotalOrders = 20, TotalRevenue = 2000m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByEndPeriod_WhenEndPeriodProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { EndPeriod = "2024-02" };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "January", TotalOrders = 10, TotalRevenue = 1000m },
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByPeriodRange_WhenBothPeriodsProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { StartPeriod = "2024-02", EndPeriod = "2024-03" };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m },
            new() { Year = 2024, Month = "March", TotalOrders = 20, TotalRevenue = 2000m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMinOrders_WhenMinOrdersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { MinOrders = 15 };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m },
            new() { Year = 2024, Month = "March", TotalOrders = 20, TotalRevenue = 2000m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMaxOrders_WhenMaxOrdersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { MaxOrders = 15 };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "January", TotalOrders = 10, TotalRevenue = 1000m },
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMinRevenue_WhenMinRevenueProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { MinRevenue = 1500m };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m },
            new() { Year = 2024, Month = "March", TotalOrders = 20, TotalRevenue = 2000m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByMaxRevenue_WhenMaxRevenueProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { MaxRevenue = 1500m };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "January", TotalOrders = 10, TotalRevenue = 1000m },
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyMultipleFilters_WhenMultipleFiltersProvided()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest
        {
            StartPeriod = "2024-01",
            EndPeriod = "2024-03",
            MinOrders = 15,
            MinRevenue = 1500m
        };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m },
            new() { Year = 2024, Month = "March", TotalOrders = 20, TotalRevenue = 2000m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldOrderByDateAscending_WhenSortByDateAsc()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { OrderBy = "date", OrderDirection = "asc" };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "January", TotalOrders = 10, TotalRevenue = 1000m },
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldOrderByTotalRevenueDescending_WhenSortByTotalRevenueDesc()
    {
        // Arrange
        var mockRepository = new Mock<IReportRepository>();
        var handler = new RevenueByPeriodHandler(mockRepository.Object);

        var request = new RevenueByPeriodRequest { OrderBy = "totalRevenue", OrderDirection = "desc" };
        var expectedData = new List<RevenueByPeriodResponse>
        {
            new() { Year = 2024, Month = "March", TotalOrders = 20, TotalRevenue = 2000m },
            new() { Year = 2024, Month = "February", TotalOrders = 15, TotalRevenue = 1500m }
        };

        mockRepository
            .Setup(repo => repo.GetRevenueByPeriodAsync(request))
            .ReturnsAsync((expectedData, expectedData.Count));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(expectedData);
        mockRepository.Verify(repo => repo.GetRevenueByPeriodAsync(request), Times.Once);
    }
}
