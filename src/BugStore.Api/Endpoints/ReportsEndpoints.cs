using BugStore.Application.Interfaces;
using BugStore.Application.Responses.Reports;
using BugStore.Application.UseCases.Reports.BestCustomers;
using BugStore.Application.UseCases.Reports.RevenueByPeriod;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints;

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/reports")
            .WithTags("Reports");

        group.MapGet("/best-customers", async ([AsParameters] BestCustomersRequest request, [FromServices] IHandler<BestCustomersRequest, GetBestCustomersResponse> handler) =>
        {
            if (request.PageNumber < 1 || request.PageSize < 1 || request.PageSize > 100)
            {
                return Results.BadRequest(new
                {
                    error = "Invalid pagination parameters. PageNumber must be >= 1 and PageSize must be between 1 and 100.",
                    pageNumber = request.PageNumber,
                    pageSize = request.PageSize
                });
            }

            var invalidOrdersRange = request.MinOrders.HasValue && request.MaxOrders.HasValue && request.MinOrders.Value > request.MaxOrders.Value;
            var invalidSpentRange = request.MinSpent.HasValue && request.MaxSpent.HasValue && request.MinSpent.Value > request.MaxSpent.Value;

            if (invalidOrdersRange || invalidSpentRange)
            {
                return Results.BadRequest(new
                {
                    error = "Invalid range filter(s). Ensure that Min is less than or equal to Max.",
                    orders = invalidOrdersRange ? new { min = request.MinOrders, max = request.MaxOrders } : null,
                    spent = invalidSpentRange ? new { min = request.MinSpent, max = request.MaxSpent } : null
                });
            }

            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapGet("/revenue-by-period", async ([AsParameters] RevenueByPeriodRequest request, [FromServices] IHandler<RevenueByPeriodRequest, GetRevenueByPeriodResponse> handler) =>
        {
            if (request.PageNumber < 1 || request.PageSize < 1 || request.PageSize > 100)
            {
                return Results.BadRequest(new
                {
                    error = "Invalid pagination parameters. PageNumber must be >= 1 and PageSize must be between 1 and 100.",
                    pageNumber = request.PageNumber,
                    pageSize = request.PageSize
                });
            }

            static bool TryParsePeriod(string? period, out int year, out int month)
            {
                year = 0; month = 0;
                if (string.IsNullOrWhiteSpace(period)) return false;
                var parts = period.Split('-');
                if (parts.Length != 2) return false;
                if (!int.TryParse(parts[0], out year) || !int.TryParse(parts[1], out month)) return false;
                if (year < 1900 || year > 9999) return false;
                if (month < 1 || month > 12) return false;
                return true;
            }

            var hasStart = !string.IsNullOrWhiteSpace(request.StartPeriod);
            var hasEnd = !string.IsNullOrWhiteSpace(request.EndPeriod);

            bool invalidFormat = false;
            DateTime? start = null; DateTime? end = null;

            if (hasStart)
            {
                if (!TryParsePeriod(request.StartPeriod, out var y, out var m)) invalidFormat = true; else start = new DateTime(y, m, 1);
            }
            if (hasEnd)
            {
                if (!TryParsePeriod(request.EndPeriod, out var y, out var m)) invalidFormat = true; else end = new DateTime(y, m, DateTime.DaysInMonth(y, m));
            }

            if (invalidFormat)
            {
                return Results.BadRequest(new { error = "Invalid period format. Use YYYY-MM for startPeriod/endPeriod.", startPeriod = request.StartPeriod, endPeriod = request.EndPeriod });
            }

            var invalidPeriodRange = start.HasValue && end.HasValue && start.Value > end.Value;
            var invalidOrdersRange = request.MinOrders.HasValue && request.MaxOrders.HasValue && request.MinOrders.Value > request.MaxOrders.Value;
            var invalidRevenueRange = request.MinRevenue.HasValue && request.MaxRevenue.HasValue && request.MinRevenue.Value > request.MaxRevenue.Value;

            if (invalidPeriodRange || invalidOrdersRange || invalidRevenueRange)
            {
                return Results.BadRequest(new
                {
                    error = "Invalid range filter(s). Ensure that Min/Start is less than or equal to Max/End.",
                    period = invalidPeriodRange ? new { start = request.StartPeriod, end = request.EndPeriod } : null,
                    orders = invalidOrdersRange ? new { min = request.MinOrders, max = request.MaxOrders } : null,
                    revenue = invalidRevenueRange ? new { min = request.MinRevenue, max = request.MaxRevenue } : null
                });
            }

            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });
    }
}
