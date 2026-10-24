namespace RequestsManagement.Domain.Requests;

public enum RequestStatus : byte
{
    New = 1,
    InProgress = 2,
    Waiting = 3,
    Completed = 4
}
