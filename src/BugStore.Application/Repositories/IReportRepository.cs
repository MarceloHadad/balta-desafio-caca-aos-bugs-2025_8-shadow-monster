using BugStore.Application.UseCases.Reports.BestCustomers;
using BugStore.Application.UseCases.Reports.RevenueByPeriod;

namespace BugStore.Application.Repositories;

public interface IReportRepository
{
    Task<(IReadOnlyList<BestCustomersResponse> Items, int TotalCount)> GetBestCustomersAsync(BestCustomersRequest request);
    Task<(IReadOnlyList<RevenueByPeriodResponse> Items, int TotalCount)> GetRevenueByPeriodAsync(RevenueByPeriodRequest request);
}
