using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Commands;

public sealed record UpdateRequestStatusRequest(RequestStatus Status, string Version);
