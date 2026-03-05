namespace Infrastructure;

public interface IEventRegistrationsRepository : IGenericRepository<EventRegistration>
{
    Task<bool> ExistsAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<List<EventRegistrationSummaryResponse>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<EventRegistrationDetailResponse?> GetDetailAsync(
        Guid registrationId,
        Guid userId,
        CancellationToken ct = default);

    Task<bool> HasCheckedInAsync(Guid eventId, Guid userId, CancellationToken ct = default);
}
