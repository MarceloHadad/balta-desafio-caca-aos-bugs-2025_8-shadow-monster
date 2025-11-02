using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Responses.Reports;
using BugStore.Application.UseCases.Reports.RevenueByPeriod;

namespace BugStore.Application.Handlers.Reports;

public class RevenueByPeriodHandler(IReportRepository reports) : IHandler<RevenueByPeriodRequest, GetRevenueByPeriodResponse>
{
    private readonly IReportRepository _reports = reports;

    public async Task<GetRevenueByPeriodResponse> HandleAsync(RevenueByPeriodRequest request)
    {
        var (items, totalCount) = await _reports.GetRevenueByPeriodAsync(request);
        var pageNumber = (request.PageNumber ?? 1);
        if (pageNumber < 1) pageNumber = 1;
        var pageSize = (request.PageSize ?? 10);
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        return new GetRevenueByPeriodResponse
        {
            Items = items.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
