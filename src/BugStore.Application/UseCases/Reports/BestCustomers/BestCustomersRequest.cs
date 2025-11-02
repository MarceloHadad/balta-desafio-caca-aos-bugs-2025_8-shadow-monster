namespace BugStore.Application.UseCases.Reports.BestCustomers;

public class BestCustomersRequest
{
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public int? MinOrders { get; set; }
    public int? MaxOrders { get; set; }
    public decimal? MinSpent { get; set; }
    public decimal? MaxSpent { get; set; }
    public string? OrderBy { get; set; }
    public string? OrderDirection { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}