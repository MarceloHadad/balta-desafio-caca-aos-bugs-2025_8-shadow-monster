namespace BugStore.Application.UseCases.Products.Search;

public class SearchProductsRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}