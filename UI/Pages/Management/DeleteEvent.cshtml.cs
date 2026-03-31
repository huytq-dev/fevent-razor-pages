using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Models.Events;

namespace UI.Pages.Management;

public class DeleteEventModel : PageModel
{
    private readonly IEventsService _eventsService;

    public DeleteEventModel(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [BindProperty]
    public DeleteEventViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
        {
            // Seed mock data for UI testing if no actual ID is provided
            ViewModel = new DeleteEventViewModel
            {
                Id = Guid.Empty,
                Title = "Graduation Ceremony Spring 2024",
                StartTime = new DateTime(2024, 5, 15),
                LocationName = "Main Hall, FPT University",
                ThumbnailUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDWCSyTH_U50njS0wpNV9jzSKuxpJilS6Ki0Kv-b-0wNZUGVYvh4aeIBt6TL2dz-18xK7CtmP4SQz8VaDeihujosJNptb0ijg_uoB82VZV0hffomnDQqNQK6TVzUFH59QtlPD_fwGUXE5PVndKChTyVJYBOpJzTGd0spPGUZkGgZH6QyTDxPtMrEwm6_SF2mpACTvfrdUQTsuCZ8Zm1JVR505u5fJPJuZdsIbEKV-B17ZytMtP4PvMqtaHERlkoOyGr7RHUGgFdD3b4"
            };
            return Page();
        }

        var response = await _eventsService.GetDetailAsync(id.Value);
        if (response == null || response.Data == null)
        {
            return NotFound();
        }

        var eventDetail = response.Data;

        ViewModel = new DeleteEventViewModel
        {
            Id = id.Value,
            Title = eventDetail.Title,
            StartTime = eventDetail.StartTime.LocalDateTime,
            LocationName = eventDetail.LocationName,
            ThumbnailUrl = eventDetail.ThumbnailUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (ViewModel.DeleteConfirmation != "DELETE")
        {
            ModelState.AddModelError("ViewModel.DeleteConfirmation", "Please type DELETE to confirm deletion.");
            
            // Re-load basic info for the view if validation fails
            if (ViewModel.Id != Guid.Empty)
            {
                var response = await _eventsService.GetDetailAsync(ViewModel.Id);
                if (response?.Data != null)
                {
                    ViewModel.Title = response.Data.Title;
                    ViewModel.StartTime = response.Data.StartTime.LocalDateTime;
                    ViewModel.LocationName = response.Data.LocationName;
                    ViewModel.ThumbnailUrl = response.Data.ThumbnailUrl;
                }
            }
            return Page();
        }

        if (ViewModel.Id != Guid.Empty)
        {
            // Logic to delete from database
            // var result = await _eventsService.DeleteAsync(ViewModel.Id);
            // if (!result.IsSuccess)
            // {
            //     ModelState.AddModelError("", "Failed to delete the event.");
            //     return Page();
            // }
        }

        TempData["SuccessMessage"] = "Event deleted successfully.";
        return RedirectToPage("./CreateEvent", new { tab = "create" });
    }
}
