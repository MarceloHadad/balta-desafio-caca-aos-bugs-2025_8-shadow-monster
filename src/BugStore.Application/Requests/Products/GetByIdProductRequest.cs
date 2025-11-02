namespace BugStore.Application.Requests.Products;

public class GetByIdProductRequest
{
    public Guid Id { get; set; }

    public GetByIdProductRequest(Guid id)
    {
        Id = id;
    }
}