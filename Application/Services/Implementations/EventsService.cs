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

        var ticketType = new TicketType
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Name = "Standard",
            Price = 0,
            Quantity = request.MaxParticipants,
            SoldCount = 0
        };
        await _unitOfWork.Repository<TicketType>().AddAsync(ticketType);

        var rowsAffected = await _unitOfWork.SaveChangesAsync(ct);

        if (rowsAffected > 0)
        {
            return PageResponse<Guid>.Ok(@event.Id);
        }

        return PageResponse<Guid>.Fail("Lỗi khi tạo sự kiện");
    }

    public async Task<PageResponse<bool>> UpdateAsync(UpdateEventRequest request, CancellationToken ct = default)
    {
        var @event = await _unitOfWork.Events.GetByIdAsync(request.Id);
        if (@event is null || @event.IsDeleted)
            return PageResponse<bool>.Fail("Event not found.");

        if (@event.OrganizerId != request.OrganizerId)
            return PageResponse<bool>.Fail("You are not allowed to update this event.");

        var editable = @event.Status is not EventStatus.Completed;
        if (!editable)
            return PageResponse<bool>.Fail("Completed events cannot be updated.");

        if (request.EndTime <= request.StartTime)
            return PageResponse<bool>.Fail("End time must be after start time.");

        var currentRegistrations = await _unitOfWork.EventRegistrations.GetByEventAsync(request.Id, ct);
        if (request.MaxParticipants < currentRegistrations.Count)
            return PageResponse<bool>.Fail("Capacity cannot be less than current registrations.");

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.ThumbnailUrl = request.ThumbnailUrl;
        @event.StartTime = request.StartTime;
        @event.EndTime = request.EndTime;
        @event.MaxParticipants = request.MaxParticipants;
        @event.CategoryId = request.CategoryId;
        @event.LocationId = request.LocationId;
        @event.MajorId = request.MajorId;
        @event.IsPrivate = request.IsPrivate;
        @event.ModifiedAt = DateTimeOffset.Now;

        _unitOfWork.Events.Update(@event);

        var existingTicket = _unitOfWork.Repository<TicketType>().GetQueryable()
            .FirstOrDefault(t => t.EventId == request.Id);
        if (existingTicket is not null)
        {
            existingTicket.Quantity = request.MaxParticipants;
            _unitOfWork.Repository<TicketType>().Update(existingTicket);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return PageResponse<bool>.Ok(true);
    }

    public async Task<PageResponse<bool>> UpdateStatusAsync(Guid id, EventStatus newStatus, CancellationToken ct = default)
    {
        var @event = await _unitOfWork.Events.GetByIdAsync(id);
        if (@event is null)
            return PageResponse<bool>.Fail("Event not found.");

        var allowed = (@event.Status, newStatus) switch
        {
            (EventStatus.Draft, EventStatus.Approved)    => true,
            (EventStatus.Pending,  EventStatus.Approved)  => true,
            (EventStatus.Pending,  EventStatus.Rejected)  => true,
            (EventStatus.Rejected, EventStatus.Approved)  => true,
            (EventStatus.Approved, EventStatus.Cancelled) => true,
            (EventStatus.Pending,  EventStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
            return PageResponse<bool>.Fail($"Cannot transition from {@event.Status} to {newStatus}.");

        @event.Status = newStatus;
        _unitOfWork.Events.Update(@event);
        await _unitOfWork.SaveChangesAsync(ct);
        return PageResponse<bool>.Ok(true);
    }

    public async Task<PageResponse<bool>> DeleteAsync(Guid id, Guid organizerId, CancellationToken ct = default)
    {
        var @event = await _unitOfWork.Events.GetByIdAsync(id);
        if (@event is null || @event.IsDeleted)
            return PageResponse<bool>.Fail("Event not found.");

        if (@event.OrganizerId != organizerId)
            return PageResponse<bool>.Fail("You are not allowed to delete this event.");

        if (@event.Status is EventStatus.Approved or EventStatus.Completed)
            return PageResponse<bool>.Fail("Approved or completed events cannot be deleted directly.");

        @event.IsDeleted = true;
        @event.DeletedAt = DateTimeOffset.Now;
        @event.Status = EventStatus.Cancelled;
        @event.ModifiedAt = DateTimeOffset.Now;

        _unitOfWork.Events.Update(@event);
        await _unitOfWork.SaveChangesAsync(ct);
        return PageResponse<bool>.Ok(true);
    }
}
