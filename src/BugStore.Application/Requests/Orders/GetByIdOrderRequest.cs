namespace BugStore.Application.Requests.Orders;

public class GetByIdOrderRequest
{
    public Guid Id { get; set; }

    public GetByIdOrderRequest(Guid id)
    {
        Id = id;
    }
}