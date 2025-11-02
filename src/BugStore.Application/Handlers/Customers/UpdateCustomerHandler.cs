using System.ComponentModel.DataAnnotations;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;

namespace BugStore.Application.Handlers.Customers;

public class UpdateCustomerHandler : IHandler<UpdateCustomerRequest, UpdateCustomerResponse>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateCustomerHandler(ICustomerRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<UpdateCustomerResponse> HandleAsync(UpdateCustomerRequest request)
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

        var existingCustomer = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Customer not found");

        var emailOwner = await _repository.GetByEmailAsync(request.Email);
        if (emailOwner is not null && emailOwner.Id != request.Id)
            throw new InvalidOperationException("Email already in use");

        existingCustomer.Name = request.Name;
        existingCustomer.Email = request.Email;
        existingCustomer.Phone = request.Phone;
        existingCustomer.BirthDate = request.BirthDate;

        await _repository.UpdateAsync(existingCustomer);
        await _uow.CommitAsync();

        var response = new UpdateCustomerResponse
        {
            Id = existingCustomer.Id,
            Name = existingCustomer.Name,
            Email = existingCustomer.Email,
            Phone = existingCustomer.Phone,
            BirthDate = existingCustomer.BirthDate
        };

        return response;
    }
}