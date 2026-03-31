namespace Contract;

public sealed class UpdateEventRequest
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public string? ThumbnailUrl { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public int MaxParticipants { get; init; }
    public Guid CategoryId { get; init; }
    public Guid LocationId { get; init; }
    public Guid? MajorId { get; init; }
    public bool IsPrivate { get; init; }
    public Guid OrganizerId { get; init; }
}
