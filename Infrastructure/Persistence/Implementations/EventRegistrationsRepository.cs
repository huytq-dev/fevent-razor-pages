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
}
