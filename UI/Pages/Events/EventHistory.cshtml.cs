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

    public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var uid))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/events/history" });
        }

        var registrations = await eventRegistrationsService.GetByUserAsync(uid, ct);

        var rows = registrations.Select((r, i) => new[]
        {
            (i + 1).ToString(),
            r.EventTitle,
            r.LocationName,
            r.StartTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            r.EndTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            GetStatusLabel(r.Status),
            r.RegisteredAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            r.TicketCode,
            r.Price.ToString("0.##")
        });

        var workbook = ExcelExportHelper.BuildWorkbook(
            ["No", "Event", "Location", "Start", "End", "Status", "Registered At", "Ticket Code", "Price"],
            rows,
            "Event History");

        var fileName = $"registration-history-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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

    private static string GetStatusLabel(int status) => (RegistrationStatus)status switch
    {
        RegistrationStatus.Confirmed => "Registered",
        RegistrationStatus.CheckedIn => "Attended",
        RegistrationStatus.PendingPayment => "Pending Payment",
        RegistrationStatus.Paid => "Paid",
        RegistrationStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
}
