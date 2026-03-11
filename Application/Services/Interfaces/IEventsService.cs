namespace Application;

public interface IEventsService
{
    Task<PageResponse<PagedResult<EventSummaryResponse>>> GetAllAsync(
        QueryInfo queryInfo,
        CancellationToken ct = default);

    Task<PageResponse<EventDetailResponse>> GetDetailAsync(Guid id, CancellationToken ct = default);
}
