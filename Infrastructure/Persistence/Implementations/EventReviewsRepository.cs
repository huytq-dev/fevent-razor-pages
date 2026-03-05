namespace Infrastructure;

[RegisterService(typeof(IEventReviewsRepository))]
public class EventReviewsRepository : GenericRepository<EventReview>, IEventReviewsRepository
{
    public EventReviewsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasReviewedAsync(Guid eventId, Guid userId, CancellationToken ct = default)
        => await _set.AsNoTracking()
            .AnyAsync(r =>
                r.EventId == eventId &&
                r.UserId == userId &&
                !r.IsDeleted, ct);

    public async Task<List<EventReviewResponse>> GetByEventAsync(Guid eventId, CancellationToken ct = default)
        => await _set.AsNoTracking()
            .Where(r => r.EventId == eventId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new EventReviewResponse
            {
                Id = r.Id,
                EventId = r.EventId,
                UserId = r.UserId,
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
}
