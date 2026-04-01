namespace UI;

public class EventApprovalModel(IEventsService eventsService, IEventRegistrationsService registrationsService) : PageModel
{
    private const int PageSizeDefault = 10;

    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public IReadOnlyList<EventSummaryResponse> Events { get; private set; } = [];
    public int TotalCount { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (TempData["SuccessMessage"] is string s) SuccessMessage = s;
        if (TempData["ErrorMessage"]   is string e) ErrorMessage   = e;
        await LoadEventsAsync(ct);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid eventId, CancellationToken ct)
    {
        var result = await eventsService.UpdateStatusAsync(eventId, EventStatus.Approved, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
            result.IsSuccess ? "Event approved." : result.Message;
        return RedirectToPage(new { StatusFilter, Search, PageNumber });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid eventId, CancellationToken ct)
    {
        var result = await eventsService.UpdateStatusAsync(eventId, EventStatus.Rejected, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
            result.IsSuccess ? "Event rejected." : result.Message;
        return RedirectToPage(new { StatusFilter, Search, PageNumber });
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid eventId, CancellationToken ct)
    {
        var result = await eventsService.UpdateStatusAsync(eventId, EventStatus.Cancelled, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
            result.IsSuccess ? "Event cancelled." : result.Message;
        return RedirectToPage(new { StatusFilter, Search, PageNumber });
    }

    public async Task<IActionResult> OnGetExportEventsAsync(CancellationToken ct)
    {
        var query = new QueryInfo
        {
            Top = 1000,
            Skip = 0,
            OrderBy = "createdAt desc",
            NeedTotalCount = false,
            IsActive = false,
            SearchText = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            Status = ParseStatusFilter(StatusFilter)
        };

        var result = await eventsService.GetAllAsync(query, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            TempData["ErrorMessage"] = "Không thể xuất báo cáo sự kiện.";
            return RedirectToPage(new { StatusFilter, Search, PageNumber });
        }


        var rows = result.Data.Items.Select((ev, i) => new[]
        {
            (i + 1).ToString(),
            ev.Title,
            ev.OrganizerName,
            ev.CategoryName,
            ev.LocationName,
            ev.StartTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            ev.EndTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            ev.RegisteredCount.ToString(),
            ev.MaxParticipants.ToString(),
            ((EventStatus)ev.Status).ToString(),
            ev.ThumbnailUrl
        });

        var workbook = ExcelExportHelper.BuildWorkbook(
            ["No", "Title", "Organizer", "Category", "Location", "Start", "End", "Registered", "Capacity", "Status", "Thumbnail URL"],
            rows,
            "Events");

        var fileName = $"events-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> OnGetExportParticipantsAsync(Guid eventId, CancellationToken ct)
    {
        var eventResult = await eventsService.GetDetailAsync(eventId, ct);
        if (!eventResult.IsSuccess || eventResult.Data is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy sự kiện.";
            return RedirectToPage(new { StatusFilter, Search, PageNumber });
        }

        var participants = await registrationsService.GetByEventAsync(eventId, ct);
        var rows = participants.Select((p, i) => new[]
        {
            (i + 1).ToString(),
            p.FullName,
            p.StudentId,
            p.Email,
            p.PhoneNumber,
            p.Major,
            p.TicketCode,
            ((RegistrationStatus)p.Status).ToString(),
            p.RegisteredAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            p.CheckInTime?.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            p.AvatarUrl
        });

        var workbook = ExcelExportHelper.BuildWorkbook(
            ["No", "Full Name", "Student ID", "Email", "Phone", "Major", "Ticket Code", "Status", "Registered At", "Check-in Time", "Avatar URL"],
            rows,
            "Participants");

        var fileName = $"participants-{eventResult.Data.Title}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName.Replace(" ", "-"));
    }

    public async Task<IActionResult> OnGetExportAttendedAsync(Guid eventId, CancellationToken ct)
    {
        var eventResult = await eventsService.GetDetailAsync(eventId, ct);
        if (!eventResult.IsSuccess || eventResult.Data is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy sự kiện.";
            return RedirectToPage(new { StatusFilter, Search, PageNumber });
        }

        var participants = await registrationsService.GetByEventAsync(eventId, ct);
        var attended = participants.Where(p => (RegistrationStatus)p.Status == RegistrationStatus.CheckedIn);

        var rows = attended.Select((p, i) => new[]
        {
            (i + 1).ToString(),
            p.FullName,
            p.StudentId,
            p.Email,
            p.PhoneNumber,
            p.Major,
            p.TicketCode,
            p.CheckInTime?.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            p.AvatarUrl
        });

        var workbook = ExcelExportHelper.BuildWorkbook(
            ["No", "Full Name", "Student ID", "Email", "Phone", "Major", "Ticket Code", "Check-in Time", "Avatar URL"],
            rows,
            "Attended");

        var fileName = $"attended-{eventResult.Data.Title}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName.Replace(" ", "-"));
    }

    private async Task LoadEventsAsync(CancellationToken ct)
    {
        var page = Math.Max(1, PageNumber);

        int? statusInt = null;
        if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<EventStatus>(StatusFilter, out var fs))
            statusInt = (int)fs;

        var query = new QueryInfo
        {
            Top = PageSizeDefault,
            Skip = (page - 1) * PageSizeDefault,
            OrderBy = "createdAt desc",
            NeedTotalCount = true,
            IsActive = false,
            SearchText = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            Status = statusInt,
        };

        var result = await eventsService.GetAllAsync(query, ct);
        if (!result.IsSuccess || result.Data is null) return;

        Events      = result.Data.Items;
        TotalCount  = result.Data.TotalCount ?? 0;
        CurrentPage = page;
        TotalPages  = TotalCount == 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSizeDefault);
    }

    private static int? ParseStatusFilter(string? statusFilter)
    {
        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<EventStatus>(statusFilter, out var status))
        {
            return (int)status;
        }

        return null;
    }
}
