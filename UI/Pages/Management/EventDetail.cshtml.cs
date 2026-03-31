using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Management;

public class EventDetailModel(IEventsService eventsService) : PageModel
{
    public EventDetailResponse? Event { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? FullName { get; private set; }
    public string? AvatarUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        FullName  = HttpContext.Session.GetString("FullName") ?? "Organizer";
        AvatarUrl = HttpContext.Session.GetString("AvatarUrl");

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return RedirectToPage("/Authentications/Login");

        var result = await eventsService.GetDetailAsync(id, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message ?? "Event not found.";
            return Page();
        }

        // Only the event owner can access this page
        if (result.Data.OrganizerId != userId)
            return Forbid();

        Event = result.Data;
        return Page();
    }
}
