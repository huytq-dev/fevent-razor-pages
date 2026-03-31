using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UI.Models.Events;

namespace UI.Pages.Management;

public class UpdateEventModel : PageModel
{
    private readonly ICatalogService _catalogService;
    private readonly IEventsService _eventsService;

    public UpdateEventModel(ICatalogService catalogService, IEventsService eventsService)
    {
        _catalogService = catalogService;
        _eventsService = eventsService;
    }

    [BindProperty]
    public UpdateEventViewModel ViewModel { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Locations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        await LoadCatalogsAsync();

        if (id == null || id == Guid.Empty)
        {
            // Seed mock data for UI testing if no actual ID is provided
            ViewModel = new UpdateEventViewModel
            {
                Title = "AI Workshop 2024",
                Description = "Join us for an in-depth look at the future of Artificial Intelligence...",
                MaxParticipants = 150,
                StartDate = DateTime.Today,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                RegistrationDeadline = DateTime.Today.AddDays(-3),
                IsPublished = true
            };
            return Page();
        }

        var response = await _eventsService.GetDetailAsync(id.Value);
        if (response == null || response.Data == null)
        {
            return NotFound();
        }

        var eventDetail = response.Data;

        // Map domain to ViewModel
        ViewModel = new UpdateEventViewModel
        {
            Id = id.Value,
            Title = eventDetail.Title,
            CategoryId = eventDetail.CategoryId,
            LocationId = eventDetail.LocationId,
            Description = eventDetail.Description ?? string.Empty,
            ThumbnailUrl = eventDetail.ThumbnailUrl,
            IsPublished = (Domain.EventStatus)eventDetail.Status == Domain.EventStatus.Approved,
            MaxParticipants = eventDetail.MaxParticipants,
            StartDate = eventDetail.StartTime.LocalDateTime.Date,
            StartTime = eventDetail.StartTime.LocalDateTime.TimeOfDay,
            EndTime = eventDetail.EndTime.LocalDateTime.TimeOfDay,
            RegistrationDeadline = eventDetail.StartTime.LocalDateTime.Date.AddDays(-3)
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCatalogsAsync();
            return Page();
        }

        // Logic for updating the event would go here, e.g.:
        // var success = await _eventsService.UpdateAsync(ViewModel);
        // if (success)
        // {
        //     return RedirectToPage("/Events/Detail", new { id = ViewModel.Id });
        // }

        TempData["SuccessMessage"] = "Event updated successfully (Mock-up)";
        return RedirectToPage("./UpdateEvent", new { id = ViewModel.Id });
    }

    private async Task LoadCatalogsAsync()
    {
        var categories = await _catalogService.GetCategoriesAsync();
        Categories = categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = c.Id == ViewModel.CategoryId
        }).ToList();

        var locations = await _catalogService.GetLocationsAsync();
        Locations = locations.Select(l => new SelectListItem
        {
            Value = l.Id.ToString(),
            Text = l.Name,
            Selected = l.Id == ViewModel.LocationId
        }).ToList();
    }
}
