namespace BugStore.Application.UseCases.Reports.RevenueByPeriod;

public class RevenueByPeriodRequest
{
    public string? StartPeriod { get; set; }
    public string? EndPeriod { get; set; }
    public string? OrderBy { get; set; }
    public string? OrderDirection { get; set; }
    public int? MinOrders { get; set; }
    public int? MaxOrders { get; set; }
    public decimal? MinRevenue { get; set; }
    public decimal? MaxRevenue { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}