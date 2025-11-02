namespace BugStore.Application.Responses.Customers;

public class GetCustomersResponse
{
    public List<GetByIdCustomerResponse> Customers { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}