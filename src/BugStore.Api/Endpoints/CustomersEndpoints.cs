using BugStore.Application.Interfaces;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;
using BugStore.Application.UseCases.Customers.Search;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints;

public static class CustomersEndpoints
{
    public static void MapCustomersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/customers")
            .WithTags("Customers");

        group.MapGet("/", async ([AsParameters] SearchCustomersRequest request, [FromServices] IHandler<SearchCustomersRequest, GetCustomersResponse> handler) =>
        {
            if (request.PageNumber.HasValue && request.PageNumber.Value < 1)
            {
                return Results.BadRequest(new { error = "Invalid pagination: PageNumber must be >= 1.", pageNumber = request.PageNumber });
            }
            if (request.PageSize.HasValue && (request.PageSize.Value < 1 || request.PageSize.Value > 100))
            {
                return Results.BadRequest(new { error = "Invalid pagination: PageSize must be between 1 and 100.", pageSize = request.PageSize });
            }

            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (Guid id, [FromServices] IHandler<GetByIdCustomerRequest, GetByIdCustomerResponse> handler) =>
        {
            var request = new GetByIdCustomerRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async ([FromServices] IHandler<CreateCustomerRequest, CreateCustomerResponse> handler, [FromBody] CreateCustomerRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/customers/{response.Id}", response);
        });

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateCustomerRequest request, [FromServices] IHandler<UpdateCustomerRequest, UpdateCustomerResponse> handler) =>
        {
            request.Id = id;
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapDelete("/{id}", async (Guid id, [FromServices] IHandler<DeleteCustomerRequest, DeleteCustomerResponse> handler) =>
        {
            var request = new DeleteCustomerRequest { Id = id };
            var response = await handler.HandleAsync(request);
            return Results.NoContent();
        });
    }
}