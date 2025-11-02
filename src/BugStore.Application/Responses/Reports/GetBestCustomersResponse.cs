using BugStore.Application.UseCases.Reports.BestCustomers;

namespace BugStore.Application.Responses.Reports;

public class GetBestCustomersResponse
{
    public List<BestCustomersResponse> Customers { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
