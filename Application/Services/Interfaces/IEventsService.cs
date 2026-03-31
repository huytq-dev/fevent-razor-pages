namespace Application;

public interface IEventsService
{
    Task<PageResponse<PagedResult<EventSummaryResponse>>> GetAllAsync(
        QueryInfo queryInfo,
        CancellationToken ct = default);

    Task<PageResponse<EventDetailResponse>> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<PageResponse<Guid>> CreateAsync(CreateEventRequest request, CancellationToken ct = default);
    Task<PageResponse<bool>> UpdateAsync(UpdateEventRequest request, CancellationToken ct = default);
    Task<PageResponse<bool>> UpdateStatusAsync(Guid id, EventStatus newStatus, CancellationToken ct = default);
    Task<PageResponse<bool>> DeleteAsync(Guid id, Guid organizerId, CancellationToken ct = default);
}
