namespace Infrastructure;

public interface IEventRegistrationsRepository : IGenericRepository<EventRegistration>
{
    Task<bool> ExistsAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<EventRegistration?> GetByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<TicketType?> GetFirstAvailableTicketTypeAsync(Guid eventId, CancellationToken ct = default);

    Task<List<EventRegistrationSummaryResponse>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<EventRegistrationDetailResponse?> GetDetailAsync(
        Guid registrationId,
        Guid userId,
        CancellationToken ct = default);

    Task<bool> HasCheckedInAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<bool> CancelAsync(Guid eventId, Guid userId, string? reason = null, CancellationToken ct = default);

    Task<List<ParticipantSummaryResponse>> GetByEventAsync(Guid eventId, CancellationToken ct = default);
}
