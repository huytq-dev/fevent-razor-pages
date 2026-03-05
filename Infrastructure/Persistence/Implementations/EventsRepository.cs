namespace Infrastructure;

[RegisterService(typeof(IEventsRepository))]
public class EventsRepository : GenericRepository<Event>, IEventsRepository
{
    public EventsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<EventSummaryResponse>> GetAllAsync(
        QueryInfo queryInfo,
        CancellationToken ct = default)
    {
        var query = _set.AsNoTracking();

        if (queryInfo.IsActive)
        {
            query = query.Where(e => !e.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(queryInfo.SearchText))
        {
            var search = queryInfo.SearchText.Trim();
            query = query.Where(e =>
                e.Title.Contains(search) ||
                (e.Description ?? string.Empty).Contains(search));
        }

        query = ApplyOrderBy(query, queryInfo.OrderBy);

        var totalCount = queryInfo.NeedTotalCount
            ? await query.CountAsync(ct)
            : (int?)null;

        var items = await query
            .Skip(queryInfo.Skip)
            .Take(queryInfo.Top)
            .Select(e => new EventSummaryResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                ThumbnailUrl = e.ThumbnailUrl,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                MaxParticipants = e.MaxParticipants,
                RegisteredCount = e.Registrations.Count,
                Status = (int)e.Status,
                IsPrivate = e.IsPrivate,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                LocationId = e.LocationId,
                LocationName = e.Location.Name,
                OrganizerId = e.OrganizerId,
                ClubId = e.ClubId,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<EventSummaryResponse>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<EventDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .Where(e => e.Id == id && !e.IsDeleted)
            .Select(e => new EventDetailResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                ThumbnailUrl = e.ThumbnailUrl,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                MaxParticipants = e.MaxParticipants,
                RegisteredCount = e.Registrations.Count,
                Status = (int)e.Status,
                IsPrivate = e.IsPrivate,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                LocationId = e.LocationId,
                LocationName = e.Location.Name,
                LocationAddress = e.Location.Address,
                LocationMapUrl = e.Location.MapUrl,
                OrganizerId = e.OrganizerId,
                OrganizerName = e.Organizer.FullName,
                OrganizerAvatarUrl = e.Organizer.AvatarUrl,
                ClubId = e.ClubId,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    private static IQueryable<Event> ApplyOrderBy(IQueryable<Event> query, string? orderBy)
    {
        var raw = (orderBy ?? string.Empty).Trim();
        var isDesc = raw.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
        var isAsc = raw.EndsWith(" asc", StringComparison.OrdinalIgnoreCase);
        var field = raw;

        if (isDesc)
        {
            field = raw[..^5].Trim();
        }
        else if (isAsc)
        {
            field = raw[..^4].Trim();
        }

        return field.ToLowerInvariant() switch
        {
            "createdat" or "createddate" or "created" or "created_time" or "createdatetime" or "createDate" =>
                isDesc ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),

            "starttime" or "start" =>
                isDesc ? query.OrderByDescending(e => e.StartTime) : query.OrderBy(e => e.StartTime),

            "endtime" or "end" =>
                isDesc ? query.OrderByDescending(e => e.EndTime) : query.OrderBy(e => e.EndTime),

            "title" =>
                isDesc ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),

            _ => query.OrderByDescending(e => e.CreatedAt)
        };
    }
}
