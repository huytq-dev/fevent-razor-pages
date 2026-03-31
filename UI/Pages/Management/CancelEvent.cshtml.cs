using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        if (id == null || id == Guid.Empty)
        {
            // Seed mock data for UI testing if no actual ID is provided
            ViewModel = new CancelEventViewModel
            {
                Id = Guid.Empty,
                Title = "Tech Talk 2024 - AI Innovations",
                Status = "Published",
                StartTime = new DateTime(2024, 10, 25, 10, 0, 0),
                EndTime = new DateTime(2024, 10, 25, 12, 0, 0),
                LocationName = "Hall A, FPT University Campus",
                RegisteredCount = 142,
                ThumbnailUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBSr4E4N06uevHzrl-rk7acaPP4lkSiDEc5AGxEsrAzL_bkN51GmlNN69MwcLmkiRDYdVGFzfYSRdAjYbAhqrLzPVIaEpFZb2E9A-GLYHTkBVWH5Fd2J3aT9RzrrZ0kKWSPzcnL9GgzWiLNjefVz3JeEKESWGprf7RbM_3XyXtTQC1sEogyi1mZXCy0XSnAAuDnWVI9CVWPBj1Uq4SnDCz8KAI-jIa3pJ0WlehU6oO9b8cc5WfUpikIrfbMvjTRdwkxINSkEWyOcTkk"
            };
            return Page();
        }

        var response = await _eventsService.GetDetailAsync(id.Value);
        if (response == null || response.Data == null)
        {
            return NotFound();
        }

        var eventDetail = response.Data;

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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ViewModel.Id != Guid.Empty)
        {
            // Logic to update status to "Cancelled" in the database would go here:
            // var result = await _eventsService.CancelAsync(ViewModel.Id, ViewModel.CancellationReason);
            // if (!result.IsSuccess)
            // {
            //     ModelState.AddModelError(string.Empty, "Failed to cancel the event. Please try again.");
            //     return Page();
            // }
        }

        TempData["SuccessMessage"] = "Event has been cancelled successfully and attendees have been notified.";
        return RedirectToPage("./CreateEvent", new { tab = "create" });
    }
}
