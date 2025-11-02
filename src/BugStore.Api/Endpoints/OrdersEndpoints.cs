using BugStore.Application.Interfaces;
using BugStore.Application.Requests.Orders;
using BugStore.Application.Responses.Orders;
using BugStore.Application.UseCases.Orders.Search;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/orders")
            .WithTags("Orders");

        group.MapGet("/", async ([AsParameters] SearchOrdersRequest request, [FromServices] IHandler<SearchOrdersRequest, GetOrdersResponse> handler) =>
        {
            var invalidProductPriceRange = request.ProductPriceStart.HasValue && request.ProductPriceEnd.HasValue && request.ProductPriceStart.Value > request.ProductPriceEnd.Value;
            var invalidCreatedAtRange = request.CreatedAtStart.HasValue && request.CreatedAtEnd.HasValue && request.CreatedAtStart.Value > request.CreatedAtEnd.Value;
            var invalidUpdatedAtRange = request.UpdatedAtStart.HasValue && request.UpdatedAtEnd.HasValue && request.UpdatedAtStart.Value > request.UpdatedAtEnd.Value;

            if (invalidProductPriceRange || invalidCreatedAtRange || invalidUpdatedAtRange)
            {
                return Results.BadRequest(new
                {
                    error = "Invalid range filter(s). Ensure that Start is less than or equal to End.",
                    productPrice = invalidProductPriceRange ? new { start = request.ProductPriceStart, end = request.ProductPriceEnd } : null,
                    createdAt = invalidCreatedAtRange ? new { start = request.CreatedAtStart, end = request.CreatedAtEnd } : null,
                    updatedAt = invalidUpdatedAtRange ? new { start = request.UpdatedAtStart, end = request.UpdatedAtEnd } : null
                });
            }

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

        group.MapGet("/{id}", async (Guid id, [FromServices] IHandler<GetByIdOrderRequest, GetByIdOrderResponse> handler) =>
        {
            var request = new GetByIdOrderRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async ([FromServices] IHandler<CreateOrderRequest, CreateOrderResponse> handler, [FromBody] CreateOrderRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/orders/{response.Id}", response);
        });
    }
}