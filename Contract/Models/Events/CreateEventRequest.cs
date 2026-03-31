namespace Contract;

public sealed class CreateEventRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public string? ThumbnailUrl { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public int MaxParticipants { get; init; }
    public Guid CategoryId { get; init; }
    public Guid LocationId { get; init; }
    public Guid OrganizerId { get; init; }
    public Guid? ClubId { get; init; }
    public int Status { get; init; } = 1; // Mặc định là Pending (1)
}
