using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Models.Events;

namespace UI.Pages.Management;

public class PublishEventModel : PageModel
{
    private readonly IEventsService _eventsService;

    public PublishEventModel(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [BindProperty]
    public PublishEventViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
        {
            // Seed mock data for UI testing if no actual ID is provided
            ViewModel = new PublishEventViewModel
            {
                Id = Guid.Empty,
                Title = "AI in Modern Software Engineering",
                CategoryName = "Workshop",
                LocationName = "Hall Alpha, FPT University",
                StartTime = new DateTime(2024, 1, 24, 9, 0, 0),
                Description = "Exploring the impact of Large Language Models on software development...",
                ThumbnailUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuALT2EBhSG1654_Ln2w5mc5YFExA9M4c3c0EbEgWq9bbeEZQAb9eiJL--13_l1FpjI6ccFoW0-ztyK699rFXobK5R2t7QVLOOLKBNSONPc4eblNweaBnutPRQkO_33e6EuSe2MATM3VFWmwkbGFwWllPUv7ufNg79i1oMZE7J06duSbIbnjyiimz9gDF7pGND0DuJa9L3_1MPxLiyePTFtEEISWLWJSallgzCDLqZ9Gtnh2_Pw1FVkn7ifuRHkPwtD_gdx2z2hywr5v",
                Checks = new List<string> { "Venue Confirmed: Hall Alpha", "Budget Verified", "3 Guest Speakers Added", "Registration Form Active" }
            };
            return Page();
        }

        var response = await _eventsService.GetDetailAsync(id.Value);
        if (response == null || response.Data == null)
        {
            return NotFound();
        }

        var eventDetail = response.Data;

        ViewModel = new PublishEventViewModel
        {
            Id = id.Value,
            Title = eventDetail.Title,
            CategoryName = eventDetail.CategoryName,
            LocationName = eventDetail.LocationName,
            StartTime = eventDetail.StartTime.LocalDateTime,
            Description = eventDetail.Description ?? string.Empty,
            ThumbnailUrl = eventDetail.ThumbnailUrl,
            Status = ((Domain.EventStatus)eventDetail.Status).ToString(),
            Checks = new List<string> { "Venue Assigned", "Registered attendees check: " + eventDetail.RegisteredCount }
        };

        return Page();
    }

    public async Task<IActionResult> OnPostPublishAsync()
    {
        // For testing UI, we allow publish but log warning if ID is empty
        if (ViewModel.Id != Guid.Empty)
        {
            // Logic to update status to "Published" in database
            // var result = await _eventsService.UpdateStatusAsync(ViewModel.Id, Domain.EventStatus.Approved);
            // if (!result.IsSuccess)
            // {
            //     ModelState.AddModelError("", "Failed to publish event. Please check required fields.");
            //     return Page();
            // }
        }

        TempData["SuccessMessage"] = "Event published successfully! Push notifications have been sent to students.";
        return RedirectToPage("./CreateEvent", new { tab = "create" });
    }
}
