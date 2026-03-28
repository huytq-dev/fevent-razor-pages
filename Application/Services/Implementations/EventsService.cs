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

    public async Task<PageResponse<Guid>> CreateAsync(CreateEventRequest request, CancellationToken ct = default)
    {
        var @event = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxParticipants = request.MaxParticipants,
            CategoryId = request.CategoryId,
            LocationId = request.LocationId,
            OrganizerId = request.OrganizerId,
            ClubId = request.ClubId,
            Status = EventStatus.Pending, // Mặc định là Pending chờ duyệt
            CreatedAt = DateTimeOffset.Now
        };

        await _unitOfWork.Events.AddAsync(@event);
        var rowsAffected = await _unitOfWork.SaveChangesAsync(ct);

        if (rowsAffected > 0)
        {
            return PageResponse<Guid>.Ok(@event.Id);
        }

        return PageResponse<Guid>.Fail("Lỗi khi tạo sự kiện");
    }
}
