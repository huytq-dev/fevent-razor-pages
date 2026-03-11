using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI;

public class EventHistoryModel(IEventRegistrationsService eventRegistrationsService) : PageModel
{
    public IReadOnlyList<EventRegistrationSummaryResponse> Registrations { get; private set; } = [];
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var uid))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/events/history" });
        }

        SuccessMessage = TempData["CancelSuccess"] as string;
        ErrorMessage = TempData["CancelError"] as string;
        Registrations = await eventRegistrationsService.GetByUserAsync(uid, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid eventId, CancellationToken ct)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var uid))
        {
            TempData["CancelError"] = "Vui lòng đăng nhập.";
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/events/history" });
        }

        var result = await eventRegistrationsService.CancelAsync(eventId, uid, null, ct);

        if (result.IsSuccess)
            TempData["CancelSuccess"] = result.Message;
        else
            TempData["CancelError"] = result.Message;

        return RedirectToPage();
    }
}
