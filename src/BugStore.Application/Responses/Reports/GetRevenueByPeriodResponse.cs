using BugStore.Application.UseCases.Reports.RevenueByPeriod;

namespace BugStore.Application.Responses.Reports;

public class GetRevenueByPeriodResponse
{
    public List<RevenueByPeriodResponse> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
