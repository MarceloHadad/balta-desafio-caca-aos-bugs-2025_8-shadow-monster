namespace BugStore.Application.Requests.Orders;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderLineRequest> Lines { get; set; } = [];
}