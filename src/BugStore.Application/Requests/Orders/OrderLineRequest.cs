namespace BugStore.Application.Requests.Orders;

public class OrderLineRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
