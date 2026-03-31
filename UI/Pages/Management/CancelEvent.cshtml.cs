using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;
using UI.Models.Events;

namespace UI.Pages.Management;

public class CancelEventModel : PageModel
{
    private readonly IEventsService _eventsService;

    public CancelEventModel(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [BindProperty]
    public CancelEventViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/CreateEvent?tab=events" });
        }

        if (id == null || id == Guid.Empty)
        {
            return RedirectToPage("/Management/CreateEvent", new { tab = "events" });
        }

        var response = await _eventsService.GetDetailAsync(id.Value);
        if (!response.IsSuccess || response.Data == null)
        {
            return NotFound();
        }

        var eventDetail = response.Data;
        if (eventDetail.OrganizerId != userId)
        {
            return Forbid();
        }

        ViewModel = new CancelEventViewModel
        {
            Id = id.Value,
            Title = eventDetail.Title,
            Status = ((Domain.EventStatus)eventDetail.Status).ToString(),
            StartTime = eventDetail.StartTime.LocalDateTime,
            EndTime = eventDetail.EndTime.LocalDateTime,
            LocationName = eventDetail.LocationName,
            RegisteredCount = eventDetail.RegisteredCount,
            ThumbnailUrl = eventDetail.ThumbnailUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/CreateEvent?tab=events" });
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var eventDetail = await _eventsService.GetDetailAsync(ViewModel.Id);
        if (!eventDetail.IsSuccess || eventDetail.Data is null)
        {
            TempData["ErrorMessage"] = eventDetail.Message;
            return RedirectToPage("/Management/CreateEvent", new { tab = "events" });
        }

        if (eventDetail.Data.OrganizerId != userId)
        {
            return Forbid();
        }

        var result = await _eventsService.UpdateStatusAsync(ViewModel.Id, EventStatus.Cancelled);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage("/Management/EventDetail", new { id = ViewModel.Id });
        }

        TempData["SuccessMessage"] = "Event has been cancelled successfully and attendees have been notified.";
        TempData["InfoMessage"] = string.IsNullOrWhiteSpace(ViewModel.CancellationReason)
            ? null
            : $"Reason sent to attendees: {ViewModel.CancellationReason}";
        return RedirectToPage("/Management/CreateEvent", new { tab = "events" });
    }
}
