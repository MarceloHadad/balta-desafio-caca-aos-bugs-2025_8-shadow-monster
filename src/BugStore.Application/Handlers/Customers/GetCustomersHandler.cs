using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Responses.Customers;
using BugStore.Application.UseCases.Customers.Search;

namespace BugStore.Application.Handlers.Customers;

public class GetCustomersHandler : IHandler<SearchCustomersRequest, GetCustomersResponse>
{
    private readonly ICustomerRepository _repository;

    public GetCustomersHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetCustomersResponse> HandleAsync(SearchCustomersRequest request)
    {
        var (customers, totalCount) = await _repository.SearchAsync(request);

        var items = customers.Select(c => new GetByIdCustomerResponse
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            BirthDate = c.BirthDate
        }).ToList();

        return new GetCustomersResponse
        {
            Customers = items,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10,
            TotalCount = totalCount
        };
    }
}