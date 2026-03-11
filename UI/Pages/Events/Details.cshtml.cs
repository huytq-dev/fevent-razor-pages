using Application;
using Contract;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI;

public class DetailsModel(
    IEventsService eventsService,
    IEventRegistrationsService eventRegistrationsService,
    IUnitOfWork unitOfWork) : PageModel
{
    [FromRoute]
    public Guid Id { get; set; }

    public EventDetailResponse? Event { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? RegisterMessage { get; private set; }
    public string? SuccessMessage { get; private set; }
    public bool IsRegistered { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var result = await eventsService.GetDetailAsync(Id, ct);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message ?? "Không tìm thấy sự kiện.";
            return Page();
        }

        Event = result.Data;
        SuccessMessage = TempData["RegisterSuccess"] as string;
        RegisterMessage = TempData["RegisterError"] as string;

        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var uid))
        {
            IsRegistered = await unitOfWork.EventRegistrations.ExistsAsync(Id, uid, ct);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync(CancellationToken ct)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var uid))
        {
            TempData["RegisterError"] = "Vui lòng đăng nhập để đăng ký sự kiện.";
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = $"/events/{Id}" });
        }

        var result = await eventRegistrationsService.RegisterAsync(Id, uid, ct);

        if (result.IsSuccess)
        {
            TempData["RegisterSuccess"] = result.Message;
        }
        else
        {
            TempData["RegisterError"] = result.Message;
        }

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken ct)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var uid))
        {
            TempData["RegisterError"] = "Vui lòng đăng nhập.";
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = $"/events/{Id}" });
        }

        var result = await eventRegistrationsService.CancelAsync(Id, uid, null, ct);

        if (result.IsSuccess)
            TempData["RegisterSuccess"] = result.Message;
        else
            TempData["RegisterError"] = result.Message;

        return RedirectToPage(new { id = Id });
    }
}
