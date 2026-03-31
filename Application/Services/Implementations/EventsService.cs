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
            MajorId = request.MajorId,
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

    public async Task<PageResponse<bool>> UpdateStatusAsync(Guid id, EventStatus newStatus, CancellationToken ct = default)
    {
        var @event = await _unitOfWork.Events.GetByIdAsync(id);
        if (@event is null)
            return PageResponse<bool>.Fail("Event not found.");

        var allowed = (@event.Status, newStatus) switch
        {
            (EventStatus.Pending,  EventStatus.Approved)  => true,
            (EventStatus.Pending,  EventStatus.Rejected)  => true,
            (EventStatus.Rejected, EventStatus.Approved)  => true,
            (EventStatus.Approved, EventStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
            return PageResponse<bool>.Fail($"Cannot transition from {@event.Status} to {newStatus}.");

        @event.Status = newStatus;
        _unitOfWork.Events.Update(@event);
        await _unitOfWork.SaveChangesAsync(ct);
        return PageResponse<bool>.Ok(true);
    }
}
