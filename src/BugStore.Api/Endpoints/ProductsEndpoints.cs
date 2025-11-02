using BugStore.Application.Interfaces;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Products;
using BugStore.Application.UseCases.Products.Search;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/products")
            .WithTags("Products");

        group.MapGet("/", async ([AsParameters] SearchProductsRequest request, [FromServices] IHandler<SearchProductsRequest, GetProductsResponse> handler) =>
        {
            if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice.Value > request.MaxPrice.Value)
            {
                return Results.BadRequest(new
                {
                    error = "Invalid price range: MinPrice cannot be greater than MaxPrice.",
                    minPrice = request.MinPrice,
                    maxPrice = request.MaxPrice
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

        group.MapGet("/{id}", async (Guid id, [FromServices] IHandler<GetByIdProductRequest, GetByIdProductResponse> handler) =>
        {
            var request = new GetByIdProductRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async ([FromServices] IHandler<CreateProductRequest, CreateProductResponse> handler, [FromBody] CreateProductRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/products/{response.Id}", response);
        });

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateProductRequest request, [FromServices] IHandler<UpdateProductRequest, UpdateProductResponse> handler) =>
        {
            request.Id = id;
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapDelete("/{id}", async (Guid id, [FromServices] IHandler<DeleteProductRequest, DeleteProductResponse> handler) =>
        {
            var request = new DeleteProductRequest { Id = id };
            var response = await handler.HandleAsync(request);
            return Results.NoContent();
        });
    }
}