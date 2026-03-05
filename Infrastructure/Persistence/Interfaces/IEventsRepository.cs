namespace Infrastructure;

public interface IEventsRepository : IGenericRepository<Event>
{
    Task<PagedResult<EventSummaryResponse>> GetAllAsync(
        QueryInfo queryInfo,
        CancellationToken ct = default);

    Task<EventDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default);
}
