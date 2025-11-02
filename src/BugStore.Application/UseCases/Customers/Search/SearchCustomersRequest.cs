namespace BugStore.Application.UseCases.Customers.Search;

public class SearchCustomersRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}