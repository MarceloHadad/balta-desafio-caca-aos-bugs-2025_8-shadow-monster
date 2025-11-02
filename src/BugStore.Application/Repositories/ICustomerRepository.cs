using BugStore.Application.UseCases.Customers.Search;
using BugStore.Domain.Entities;

namespace BugStore.Application.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer> AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> SearchAsync(SearchCustomersRequest request);
}