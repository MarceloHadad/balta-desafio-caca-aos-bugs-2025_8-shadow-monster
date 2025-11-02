using System.ComponentModel.DataAnnotations;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;
using BugStore.Domain.Entities;

namespace BugStore.Application.Handlers.Customers;

public class CreateCustomerHandler : IHandler<CreateCustomerRequest, CreateCustomerResponse>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _uow;

    public CreateCustomerHandler(ICustomerRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<CreateCustomerResponse> HandleAsync(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ArgumentException("Phone is required");
        if (request.BirthDate == default)
            throw new ArgumentException("BirthDate is required");
        if (request.BirthDate > DateTime.UtcNow.Date)
            throw new ArgumentException("BirthDate cannot be in the future");

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(request.Email))
            throw new ArgumentException("Email is invalid");

        var emailInUse = await _repository.GetByEmailAsync(request.Email) != null;
        if (emailInUse)
            throw new InvalidOperationException("Email already in use");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            BirthDate = request.BirthDate
        };

        await _repository.AddAsync(customer);
        await _uow.CommitAsync();

        var response = new CreateCustomerResponse
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