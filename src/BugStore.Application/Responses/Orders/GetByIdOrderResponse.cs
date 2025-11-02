namespace BugStore.Application.Responses.Orders;

public class GetByIdOrderResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderLineResponse> Lines { get; set; } = [];
}