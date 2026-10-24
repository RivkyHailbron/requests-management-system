using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests;

public static class RequestMappings
{
    public static RequestDto ToDto(this Request request) =>
        new(
            request.Id,
            request.Title,
            request.OrganizationName,
            request.Status,
            request.Priority,
            request.AssignedTo,
            request.CreatedAt,
            request.UpdatedAt,
            RowVersionEncoding.Encode(request.RowVersion));
}
