namespace UI;

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
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/CreateEvent?tab=events" });
        }

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

        if (ViewModel.Id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Invalid event id.";
            return RedirectToPage("/Management/CreateEvent", new { tab = "events" });
        }

        var result = await _eventsService.DeleteAsync(ViewModel.Id, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage("/Management/EventDetail", new { id = ViewModel.Id });
        }

        TempData["SuccessMessage"] = "Event deleted successfully.";
        return RedirectToPage("/Management/CreateEvent", new { tab = "events" });
    }
}
