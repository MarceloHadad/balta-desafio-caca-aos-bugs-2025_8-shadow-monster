namespace BugStore.Application.Responses.Orders;

public class CreateOrderResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderLineResponse> Lines { get; set; } = [];
}