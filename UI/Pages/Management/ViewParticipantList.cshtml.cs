using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Models.Events;

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
                Mssv             = p.StudentId ?? "—",
                Major            = p.Major ?? "—",
                Email            = p.Email ?? "—",
                Phone            = p.PhoneNumber ?? "—",
                RegistrationDate = p.RegisteredAt.LocalDateTime,
                Status           = GetStatusLabel(p.Status),
            }).ToList()
        };

        return Page();
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
