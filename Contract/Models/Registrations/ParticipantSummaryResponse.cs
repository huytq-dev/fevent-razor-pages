namespace Contract;

public sealed class ParticipantSummaryResponse
{
    public Guid RegistrationId { get; init; }
    public Guid UserId { get; init; }
    public required string FullName { get; init; }
    public string? StudentId { get; init; }
    public string? Major { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public string TicketCode { get; init; } = null!;
    public DateTimeOffset RegisteredAt { get; init; }
    public int Status { get; init; }
    public DateTimeOffset? CheckInTime { get; init; }
}
