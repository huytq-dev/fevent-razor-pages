namespace Infrastructure;

[RegisterService(typeof(IEventRegistrationsRepository))]
public class EventRegistrationsRepository : GenericRepository<EventRegistration>, IEventRegistrationsRepository
{
    public EventRegistrationsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(Guid eventId, Guid userId, CancellationToken ct = default)
        => await _set.AsNoTracking()
            .AnyAsync(r =>
                r.EventId == eventId &&
                r.UserId == userId &&
                r.Status != RegistrationStatus.Cancelled, ct);

    public async Task<EventRegistration?> GetByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken ct = default)
        => await _set
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId, ct);

    public async Task<TicketType?> GetFirstAvailableTicketTypeAsync(Guid eventId, CancellationToken ct = default)
        => await _context.TicketTypes
            .Where(t => t.EventId == eventId && t.SoldCount < t.Quantity)
            .OrderBy(t => t.Price)
            .FirstOrDefaultAsync(ct);

    public async Task<List<EventRegistrationSummaryResponse>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _set.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new EventRegistrationSummaryResponse
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event.Title,
                EventThumbnailUrl = r.Event.ThumbnailUrl,
                StartTime = r.Event.StartTime,
                EndTime = r.Event.EndTime,
                LocationName = r.Event.Location.Name,
                RegisteredAt = r.CreatedAt,
                Status = (int)r.Status,
                ParticipantAvatarUrl = r.User.AvatarUrl,
                TicketCode = r.TicketCode,
                QrCodeUrl = r.QrCodeUrl,
                Price = r.TicketType.Price
            })
            .ToListAsync(ct);

    public async Task<EventRegistrationDetailResponse?> GetDetailAsync(
        Guid registrationId,
        Guid userId,
        CancellationToken ct = default)
        => await _set.AsNoTracking()
            .Where(r => r.Id == registrationId && r.UserId == userId)
            .Select(r => new EventRegistrationDetailResponse
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event.Title,
                EventThumbnailUrl = r.Event.ThumbnailUrl,
                StartTime = r.Event.StartTime,
                EndTime = r.Event.EndTime,
                LocationName = r.Event.Location.Name,
                LocationAddress = r.Event.Location.Address,
                OrganizerName = r.Event.Organizer.FullName,
                OrganizerAvatarUrl = r.Event.Organizer.AvatarUrl,
                RegisteredAt = r.CreatedAt,
                Status = (int)r.Status,
                TicketCode = r.TicketCode,
                QrCodeUrl = r.QrCodeUrl,
                Price = r.TicketType.Price
            })
            .FirstOrDefaultAsync(ct);

    public async Task<bool> HasCheckedInAsync(Guid eventId, Guid userId, CancellationToken ct = default)
        => await _set.AsNoTracking()
            .AnyAsync(r =>
                r.EventId == eventId &&
                r.UserId == userId &&
                r.Status == RegistrationStatus.CheckedIn, ct);

    public async Task<List<ParticipantSummaryResponse>> GetByEventAsync(Guid eventId, CancellationToken ct = default)
        => await _set.AsNoTracking()
            .Where(r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ParticipantSummaryResponse
            {
                RegistrationId = r.Id,
                UserId         = r.UserId,
                FullName       = r.User.FullName,
                StudentId      = r.User.StudentId,
                Major          = r.User.Major,
                Email          = r.User.Email,
                PhoneNumber    = r.User.PhoneNumber,
                AvatarUrl      = r.User.AvatarUrl,
                TicketCode     = r.TicketCode,
                RegisteredAt   = r.CreatedAt,
                Status         = (int)r.Status,
                CheckInTime    = r.CheckInTime,
            })
            .ToListAsync(ct);

    public async Task<bool> CancelAsync(Guid eventId, Guid userId, string? reason = null, CancellationToken ct = default)
    {
        var reg = await _set
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r =>
                r.EventId == eventId &&
                r.UserId == userId &&
                r.Status != RegistrationStatus.Cancelled &&
                r.Status != RegistrationStatus.CheckedIn, ct);
        if (reg is null)
            return false;

        reg.Status = RegistrationStatus.Cancelled;
        reg.CancelledAt = DateTimeOffset.UtcNow;
        reg.CancellationReason = reason;
        if (reg.TicketType.SoldCount > 0)
            reg.TicketType.SoldCount--;
        return true;
    }
}
