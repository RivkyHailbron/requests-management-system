using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Application.Requests.Queries;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Infrastructure.Requests;

internal static class RequestQueryExtensions
{
    public static IQueryable<Request> ApplyFilters(this IQueryable<Request> query, RequestQueryParameters parameters)
    {
        if (parameters.Status is { Count: > 0 } statuses)
        {
            query = query.Where(request => statuses.Contains(request.Status));
        }

        if (parameters.Priority is { Count: > 0 } priorities)
        {
            query = query.Where(request => priorities.Contains(request.Priority));
        }

        if (!string.IsNullOrWhiteSpace(parameters.OrganizationName))
        {
            var organizationName = parameters.OrganizationName.Trim();
            query = query.Where(request => request.OrganizationName == organizationName);
        }

        if (!string.IsNullOrWhiteSpace(parameters.AssignedTo))
        {
            var assignedTo = parameters.AssignedTo.Trim();
            query = query.Where(request => request.AssignedTo == assignedTo);
        }

        if (parameters.CreatedFrom is { } createdFrom)
        {
            query = query.Where(request => request.CreatedAt >= createdFrom);
        }

        if (parameters.CreatedTo is { } createdTo)
        {
            query = query.Where(request => request.CreatedAt <= createdTo);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(request => request.Title.Contains(search) || request.OrganizationName.Contains(search));
        }

        return query;
    }

    public static IQueryable<Request> ApplySorting(
        this IQueryable<Request> query,
        RequestSortField sortBy,
        SortDirection direction)
    {
        var ascending = direction == SortDirection.Asc;

        var ordered = sortBy switch
        {
            RequestSortField.UpdatedAt => ascending
                ? query.OrderBy(request => request.UpdatedAt)
                : query.OrderByDescending(request => request.UpdatedAt),
            RequestSortField.Priority => ascending
                ? query.OrderBy(request => request.Priority)
                : query.OrderByDescending(request => request.Priority),
            RequestSortField.Status => ascending
                ? query.OrderBy(request => request.Status)
                : query.OrderByDescending(request => request.Status),
            RequestSortField.Title => ascending
                ? query.OrderBy(request => request.Title)
                : query.OrderByDescending(request => request.Title),
            RequestSortField.OrganizationName => ascending
                ? query.OrderBy(request => request.OrganizationName)
                : query.OrderByDescending(request => request.OrganizationName),
            _ => ascending
                ? query.OrderBy(request => request.CreatedAt)
                : query.OrderByDescending(request => request.CreatedAt)
        };

        return ascending
            ? ordered.ThenBy(request => request.Id)
            : ordered.ThenByDescending(request => request.Id);
    }

    public static IQueryable<RequestDto> ProjectToDto(this IQueryable<Request> query) =>
        query.Select(request => new RequestDto(
            request.Id,
            request.Title,
            request.OrganizationName,
            request.Status,
            request.Priority,
            request.AssignedTo,
            request.CreatedAt,
            request.UpdatedAt,
            Convert.ToBase64String(request.RowVersion)));
}
