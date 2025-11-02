using BugStore.Api.Endpoints;
using BugStore.Application.Handlers.Customers;
using BugStore.Application.Handlers.Orders;
using BugStore.Application.Handlers.Products;
using BugStore.Application.Handlers.Reports;
using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Requests.Orders;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Customers;
using BugStore.Application.Responses.Orders;
using BugStore.Application.Responses.Products;
using BugStore.Application.Responses.Reports;
using BugStore.Application.UseCases.Customers.Search;
using BugStore.Application.UseCases.Orders.Search;
using BugStore.Application.UseCases.Products.Search;
using BugStore.Application.UseCases.Reports.BestCustomers;
using BugStore.Application.UseCases.Reports.RevenueByPeriod;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderLineRepository, OrderLineRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IHandler<CreateCustomerRequest, CreateCustomerResponse>, CreateCustomerHandler>();
builder.Services.AddScoped<IHandler<SearchCustomersRequest, GetCustomersResponse>, GetCustomersHandler>();
builder.Services.AddScoped<IHandler<GetByIdCustomerRequest, GetByIdCustomerResponse>, GetByIdCustomerHandler>();
builder.Services.AddScoped<IHandler<UpdateCustomerRequest, UpdateCustomerResponse>, UpdateCustomerHandler>();
builder.Services.AddScoped<IHandler<DeleteCustomerRequest, DeleteCustomerResponse>, DeleteCustomerHandler>();

builder.Services.AddScoped<IHandler<CreateProductRequest, CreateProductResponse>, CreateProductHandler>();
builder.Services.AddScoped<IHandler<SearchProductsRequest, GetProductsResponse>, GetProductsHandler>();
builder.Services.AddScoped<IHandler<GetByIdProductRequest, GetByIdProductResponse>, GetByIdProductHandler>();
builder.Services.AddScoped<IHandler<DeleteProductRequest, DeleteProductResponse>, DeleteProductHandler>();
builder.Services.AddScoped<IHandler<UpdateProductRequest, UpdateProductResponse>, UpdateProductHandler>();

builder.Services.AddScoped<IHandler<CreateOrderRequest, CreateOrderResponse>, CreateOrderHandler>();
builder.Services.AddScoped<IHandler<SearchOrdersRequest, GetOrdersResponse>, GetOrdersHandler>();
builder.Services.AddScoped<IHandler<GetByIdOrderRequest, GetByIdOrderResponse>, GetByIdOrderHandler>();

builder.Services.AddScoped<IHandler<BestCustomersRequest, GetBestCustomersResponse>, BestCustomersHandler>();
builder.Services.AddScoped<IHandler<RevenueByPeriodRequest, GetRevenueByPeriodResponse>, RevenueByPeriodHandler>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (KeyNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.MapCustomersEndpoints();
app.MapProductsEndpoints();
app.MapOrdersEndpoints();
app.MapReportsEndpoints();

app.Run();
