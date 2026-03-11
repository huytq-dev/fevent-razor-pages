namespace Application;

public interface IEventRegistrationsService
{
    Task<PageResponse> RegisterAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<EventRegistrationSummaryResponse>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    Task<PageResponse> CancelAsync(Guid eventId, Guid userId, string? reason = null, CancellationToken ct = default);
}
