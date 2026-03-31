using Domain;

namespace Application;

[RegisterService(typeof(IEventRegistrationsService))]
public class EventRegistrationsService(IUnitOfWork unitOfWork) : IEventRegistrationsService
{
    public async Task<PageResponse> RegisterAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var eventDetail = await unitOfWork.Events.GetDetailAsync(eventId, ct);
        if (eventDetail is null)
            return PageResponse.Fail("Không tìm thấy sự kiện.");

        if (eventDetail.Status != (int)EventStatus.Approved)
            return PageResponse.Fail("Sự kiện chưa mở đăng ký.");

            var existing = await unitOfWork.EventRegistrations.GetByEventAndUserAsync(eventId, userId, ct);
        if (existing is not null && existing.Status != RegistrationStatus.Cancelled)
            return PageResponse.Fail("Bạn đã đăng ký sự kiện này rồi.");

        var ticketType = await unitOfWork.EventRegistrations.GetFirstAvailableTicketTypeAsync(eventId, ct);
        if (ticketType is null)
            return PageResponse.Fail("Hết vé cho sự kiện này.");

        if (eventDetail.RegisteredCount >= eventDetail.MaxParticipants)
            return PageResponse.Fail("Sự kiện đã đủ số lượng người tham gia.");

        var status = ticketType.Price == 0 ? RegistrationStatus.Confirmed : RegistrationStatus.PendingPayment;
        var ticketCode = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

        if (existing is not null && existing.Status == RegistrationStatus.Cancelled)
        {
            // Re-activate cancelled registration (unique index prevents insert)
            existing.TicketTypeId = ticketType.Id;
            existing.TicketCode = ticketCode;
            existing.Status = status;
            existing.CancelledAt = null;
            existing.CancellationReason = null;
            ticketType.SoldCount++;
        }
        else
        {
            var registration = new EventRegistration
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                TicketTypeId = ticketType.Id,
                TicketCode = ticketCode,
                Status = status,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await unitOfWork.EventRegistrations.AddAsync(registration);
            ticketType.SoldCount++;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return PageResponse.Ok("Đăng ký thành công!");
    }

    public async Task<IReadOnlyList<EventRegistrationSummaryResponse>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await unitOfWork.EventRegistrations.GetByUserAsync(userId, ct);

    public async Task<IReadOnlyList<ParticipantSummaryResponse>> GetByEventAsync(Guid eventId, CancellationToken ct = default)
        => await unitOfWork.EventRegistrations.GetByEventAsync(eventId, ct);

    public async Task<PageResponse> CancelAsync(Guid eventId, Guid userId, string? reason = null, CancellationToken ct = default)
    {
        var cancelled = await unitOfWork.EventRegistrations.CancelAsync(eventId, userId, reason, ct);
        if (!cancelled)
            return PageResponse.Fail("Không thể hủy đăng ký. Vé có thể đã được check-in hoặc không tồn tại.");
        await unitOfWork.SaveChangesAsync(ct);
        return PageResponse.Ok("Đã hủy đăng ký sự kiện.");
    }
}
