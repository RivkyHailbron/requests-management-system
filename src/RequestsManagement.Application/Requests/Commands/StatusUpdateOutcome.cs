namespace RequestsManagement.Application.Requests.Commands;

public enum StatusUpdateOutcome
{
    Updated,
    NotFound,
    Conflict,
    InvalidTransition
}
