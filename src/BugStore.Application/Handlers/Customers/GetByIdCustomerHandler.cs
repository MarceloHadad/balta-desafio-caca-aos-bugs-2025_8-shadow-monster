using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;

namespace BugStore.Application.Handlers.Customers;

public class GetByIdCustomerHandler : IHandler<GetByIdCustomerRequest, GetByIdCustomerResponse>
{
    private readonly ICustomerRepository _repository;

    public GetByIdCustomerHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetByIdCustomerResponse> HandleAsync(GetByIdCustomerRequest request)
    {
        var customer = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Customer not found");

        var response = new GetByIdCustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            BirthDate = customer.BirthDate
        };

        return response;
    }
}