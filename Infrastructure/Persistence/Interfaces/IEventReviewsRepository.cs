namespace Infrastructure;

public interface IEventReviewsRepository : IGenericRepository<EventReview>
{
    Task<bool> HasReviewedAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<List<EventReviewResponse>> GetByEventAsync(Guid eventId, CancellationToken ct = default);
}
