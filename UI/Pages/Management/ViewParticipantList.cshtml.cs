using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Models.Events;
using UI.Helpers;

namespace UI.Pages.Management;

public class ViewParticipantListModel(
    IEventsService eventsService,
    IEventRegistrationsService registrationsService) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? MajorFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }

    public ParticipantListViewModel ViewModel { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid? eventId, CancellationToken ct)
    {
        if (eventId == null || eventId == Guid.Empty)
            return BadRequest("Event ID is required.");

        // Verify event exists and current user is the organizer
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return RedirectToPage("/Authentications/Login");

        var eventResult = await eventsService.GetDetailAsync(eventId.Value, ct);
        if (!eventResult.IsSuccess || eventResult.Data is null)
            return NotFound();

        if (eventResult.Data.OrganizerId != userId)
            return Forbid();

        var participants = await registrationsService.GetByEventAsync(eventId.Value, ct);

        // Apply search filter
        var filtered = participants.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim().ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.FullName.ToLowerInvariant().Contains(term) ||
                (p.StudentId ?? "").ToLowerInvariant().Contains(term) ||
                (p.Email ?? "").ToLowerInvariant().Contains(term) ||
                (p.TicketCode ?? "").ToLowerInvariant().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(MajorFilter))
        {
            filtered = filtered.Where(p =>
                (p.Major ?? "").Equals(MajorFilter.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            filtered = filtered.Where(p => GetStatusLabel(p.Status)
                .Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        var list = filtered.ToList();

        var majorOptions = participants
            .Select(p => p.Major)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(m => m!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ViewModel = new ParticipantListViewModel
        {
            EventId         = eventId.Value,
            EventTitle      = eventResult.Data.Title,
            TotalRegistered = participants.Count,
            MajorFilterOptions = majorOptions,
            Participants    = list.Select((p, i) => new ParticipantViewModel
            {
                Order            = i + 1,
                FullName         = p.FullName,
                AvatarUrl        = p.AvatarUrl,
                Mssv             = p.StudentId ?? "—",
                Major            = p.Major ?? "—",
                Email            = p.Email ?? "—",
                Phone            = p.PhoneNumber ?? "—",
                RegistrationDate = p.RegisteredAt.LocalDateTime,
                Status           = GetStatusLabel(p.Status),
                CheckInTime      = p.CheckInTime?.LocalDateTime,
            }).ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnGetExportAsync(Guid eventId, CancellationToken ct)
    {
        var access = await EnsureOrganizerAccessAsync(eventId, ct);
        if (access.Result is not null)
        {
            return access.Result;
        }

        var participants = access.Participants;
        var rows = participants.Select((p, i) => new[]
        {
            (i + 1).ToString(),
            p.FullName,
            p.StudentId,
            p.Email,
            p.PhoneNumber,
            p.Major,
            p.TicketCode,
            GetStatusLabel(p.Status),
            p.RegisteredAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            p.CheckInTime?.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            p.AvatarUrl
        });

        var workbook = ExcelExportHelper.BuildWorkbook(
            ["No", "Full Name", "Student ID", "Email", "Phone", "Major", "Ticket Code", "Status", "Registered At", "Check-in Time", "Avatar URL"],
            rows,
            "Participants");

        var fileName = $"participants-{eventId:N}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> OnGetExportAttendedAsync(Guid eventId, CancellationToken ct)
    {
        var access = await EnsureOrganizerAccessAsync(eventId, ct);
        if (access.Result is not null)
        {
            return access.Result;
        }

        var attended = access.Participants
            .Where(p => (Domain.RegistrationStatus)p.Status == Domain.RegistrationStatus.CheckedIn)
            .ToList();

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

        var fileName = $"attended-{eventId:N}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private async Task<(IActionResult? Result, IReadOnlyList<ParticipantSummaryResponse> Participants)> EnsureOrganizerAccessAsync(Guid eventId, CancellationToken ct)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return (RedirectToPage("/Authentications/Login"), []);
        }

        var eventResult = await eventsService.GetDetailAsync(eventId, ct);
        if (!eventResult.IsSuccess || eventResult.Data is null)
        {
            return (NotFound(), []);
        }

        if (eventResult.Data.OrganizerId != userId)
        {
            return (Forbid(), []);
        }

        var participants = await registrationsService.GetByEventAsync(eventId, ct);
        return (null, participants);
    }

    private static string GetStatusLabel(int status) => (Domain.RegistrationStatus)status switch
    {
        Domain.RegistrationStatus.Confirmed      => "Registered",
        Domain.RegistrationStatus.CheckedIn      => "Checked-in",
        Domain.RegistrationStatus.Cancelled      => "Cancelled",
        Domain.RegistrationStatus.PendingPayment => "Pending Payment",
        Domain.RegistrationStatus.Paid           => "Paid",
        _                                        => "Unknown"
    };
}
