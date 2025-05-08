using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MyIdentityApi.Dtos.Common;

namespace MyIdentityApi.Extensions;

public static class QueryableExtensions
{
    public static async Task<PaginatedResponseDto<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        var totalCount = await source.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponseDto<T>
        {
            Data = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string? sortBy, bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return source;

        var property = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var orderByExp = Expression.Lambda(propertyAccess, parameter);

        var methodName = isDescending ? "OrderByDescending" : "OrderBy";
        var resultExp = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.PropertyType },
            source.Expression,
            Expression.Quote(orderByExp));

        return source.Provider.CreateQuery<T>(resultExp);
    }
}