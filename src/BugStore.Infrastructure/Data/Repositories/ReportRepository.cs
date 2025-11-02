using System.Globalization;
using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Reports.BestCustomers;
using BugStore.Application.UseCases.Reports.RevenueByPeriod;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Data.Repositories;

public class ReportRepository(AppDbContext context) : IReportRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(IReadOnlyList<BestCustomersResponse> Items, int TotalCount)> GetBestCustomersAsync(BestCustomersRequest request)
    {
        var baseQuery = _context.Orders
            .AsNoTracking()
            .Select(o => new
            {
                o.CustomerId,
                CustomerName = o.Customer.Name,
                CustomerEmail = o.Customer.Email,
                OrderTotal = o.Lines.Sum(l => (decimal?)l.Total) ?? 0m
            });

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            var value = request.CustomerName.Trim().ToLower();
            baseQuery = baseQuery.Where(x => EF.Functions.Like(x.CustomerName.ToLower(), $"%{value}%"));
        }
        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            var value = request.CustomerEmail.Trim().ToLower();
            baseQuery = baseQuery.Where(x => EF.Functions.Like(x.CustomerEmail.ToLower(), $"%{value}%"));
        }

        var groupedQuery = await baseQuery
            .GroupBy(x => new { x.CustomerId, x.CustomerName, x.CustomerEmail })
            .Select(g => new BestCustomersResponse
            {
                CustomerName = g.Key.CustomerName,
                CustomerEmail = g.Key.CustomerEmail,
                TotalOrders = g.Count(),
                SpentAmount = g.Sum(x => x.OrderTotal)
            })
            .ToListAsync();

        var items = groupedQuery.AsEnumerable();

        if (request.MinOrders.HasValue)
            items = items.Where(i => i.TotalOrders >= request.MinOrders.Value);

        if (request.MaxOrders.HasValue)
            items = items.Where(i => i.TotalOrders <= request.MaxOrders.Value);

        if (request.MinSpent.HasValue)
            items = items.Where(i => i.SpentAmount >= request.MinSpent.Value);

        if (request.MaxSpent.HasValue)
            items = items.Where(i => i.SpentAmount <= request.MaxSpent.Value);

        var orderBy = (request.OrderBy ?? "spentAmount").ToLowerInvariant();
        var direction = (request.OrderDirection ?? "desc").ToLowerInvariant();

        if (orderBy == "totalorders" || orderBy == "orders")
        {
            items = direction == "asc"
                ? items.OrderBy(i => i.TotalOrders)
                : items.OrderByDescending(i => i.TotalOrders);
        }
        else
        {
            items = direction == "asc"
                ? items.OrderBy(i => i.SpentAmount)
                : items.OrderByDescending(i => i.SpentAmount);
        }

        var totalCount = items.Count();
        var pageNumber = (request.PageNumber ?? 1);
        if (pageNumber < 1) pageNumber = 1;
        var pageSize = (request.PageSize ?? 10);
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var paginatedItems = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (paginatedItems, totalCount);
    }

    public async Task<(IReadOnlyList<RevenueByPeriodResponse> Items, int TotalCount)> GetRevenueByPeriodAsync(RevenueByPeriodRequest request)
    {
        var baseQuery = _context.Orders
            .AsNoTracking()
            .Select(o => new
            {
                o.Id,
                o.CreatedAt,
                OrderTotal = o.Lines.Sum(l => (decimal?)l.Total) ?? 0m
            });

        DateTime? startDate = null;
        DateTime? endDate = null;

        if (!string.IsNullOrWhiteSpace(request.StartPeriod))
        {
            if (TryParsePeriod(request.StartPeriod, out var year, out var month))
            {
                startDate = new DateTime(year, month, 1);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.EndPeriod))
        {
            if (TryParsePeriod(request.EndPeriod, out var year, out var month))
            {
                var lastDay = DateTime.DaysInMonth(year, month);
                endDate = new DateTime(year, month, lastDay, 23, 59, 59, 999, DateTimeKind.Unspecified);
            }
        }

        if (startDate.HasValue)
            baseQuery = baseQuery.Where(x => x.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            baseQuery = baseQuery.Where(x => x.CreatedAt <= endDate.Value);

        var aggregated = await baseQuery
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                MonthNumber = g.Key.Month,
                TotalOrders = g.Count(),
                TotalRevenue = g.Sum(x => x.OrderTotal)
            })
            .ToListAsync();

        var items = aggregated.AsEnumerable();

        if (request.MinOrders.HasValue)
            items = items.Where(i => i.TotalOrders >= request.MinOrders.Value);

        if (request.MaxOrders.HasValue)
            items = items.Where(i => i.TotalOrders <= request.MaxOrders.Value);

        if (request.MinRevenue.HasValue)
            items = items.Where(i => i.TotalRevenue >= request.MinRevenue.Value);

        if (request.MaxRevenue.HasValue)
            items = items.Where(i => i.TotalRevenue <= request.MaxRevenue.Value);

        var orderBy = (request.OrderBy ?? "date").ToLowerInvariant();
        var direction = (request.OrderDirection ?? "asc").ToLowerInvariant();

        if (orderBy == "totalorders" || orderBy == "orders")
        {
            items = direction == "asc" ? items.OrderBy(i => i.TotalOrders) : items.OrderByDescending(i => i.TotalOrders);
        }
        else if (orderBy == "totalrevenue" || orderBy == "revenue")
        {
            items = direction == "asc" ? items.OrderBy(i => i.TotalRevenue) : items.OrderByDescending(i => i.TotalRevenue);
        }
        else
        {
            items = direction == "desc"
                ? items.OrderByDescending(i => i.Year).ThenByDescending(i => i.MonthNumber)
                : items.OrderBy(i => i.Year).ThenBy(i => i.MonthNumber);
        }

        var totalCount = items.Count();
        var pageNumber = (request.PageNumber ?? 1);
        if (pageNumber < 1) pageNumber = 1;
        var pageSize = (request.PageSize ?? 10);
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new RevenueByPeriodResponse
            {
                Year = i.Year,
                Month = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(i.MonthNumber),
                TotalOrders = i.TotalOrders,
                TotalRevenue = i.TotalRevenue
            })
            .ToList();

        return (result, totalCount);
    }

    private static bool TryParsePeriod(string period, out int year, out int month)
    {
        year = 0;
        month = 0;

        if (string.IsNullOrWhiteSpace(period))
            return false;

        var parts = period.Split('-');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out year) || !int.TryParse(parts[1], out month))
            return false;

        if (year < 1900 || year > 9999)
            return false;

        if (month < 1 || month > 12)
            return false;

        return true;
    }
}
