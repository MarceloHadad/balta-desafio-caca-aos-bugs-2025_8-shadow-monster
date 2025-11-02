namespace BugStore.Application.Responses.Products;

public class GetProductsResponse
{
    public List<GetByIdProductResponse> Products { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}