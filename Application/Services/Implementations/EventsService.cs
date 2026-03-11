namespace Application;

[RegisterService(typeof(IEventsService))]
public class EventsService(IUnitOfWork _unitOfWork) : IEventsService
{
    public async Task<PageResponse<PagedResult<EventSummaryResponse>>> GetAllAsync(
        QueryInfo queryInfo,
        CancellationToken ct = default)
    {
        var result = await _unitOfWork.Events.GetAllAsync(queryInfo, ct);
        return PageResponse<PagedResult<EventSummaryResponse>>.Ok(result);
    }

    public async Task<PageResponse<EventDetailResponse>> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _unitOfWork.Events.GetDetailAsync(id, ct);

        if (result is null)
            return PageResponse<EventDetailResponse>.Fail("Lỗi khi lấy chi tiết envent");

        return PageResponse<EventDetailResponse>.Ok(result);
    }
}
