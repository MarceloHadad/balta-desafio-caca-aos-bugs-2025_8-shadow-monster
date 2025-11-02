using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;

namespace BugStore.Application.Handlers.Customers;

public class DeleteCustomerHandler : IHandler<DeleteCustomerRequest, DeleteCustomerResponse>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteCustomerHandler(ICustomerRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<DeleteCustomerResponse> HandleAsync(DeleteCustomerRequest request)
    {
        var exists = await _repository.GetByIdAsync(request.Id) != null;
        if (!exists)
            throw new KeyNotFoundException("Customer not found");

        await _repository.DeleteAsync(request.Id);
        await _uow.CommitAsync();

        return new DeleteCustomerResponse();
    }
}