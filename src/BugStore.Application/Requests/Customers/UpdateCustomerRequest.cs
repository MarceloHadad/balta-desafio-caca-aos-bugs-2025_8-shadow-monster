namespace BugStore.Application.Requests.Customers;

public class UpdateCustomerRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }

    public UpdateCustomerRequest(Guid id)
    {
        Id = id;
    }
}