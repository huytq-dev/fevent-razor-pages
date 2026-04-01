namespace UI;

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

        ViewModel = new PublishEventViewModel
        {
            Id = id.Value,
            Title = eventDetail.Title,
            CategoryName = eventDetail.CategoryName,
            LocationName = eventDetail.LocationName,
            StartTime = eventDetail.StartTime.LocalDateTime,
            Description = eventDetail.Description ?? string.Empty,
            ThumbnailUrl = eventDetail.ThumbnailUrl,
            Status = ((EventStatus)eventDetail.Status).ToString(),
            Checks = new List<string> { "Venue Assigned", "Registered attendees check: " + eventDetail.RegisteredCount }
        };

        return Page();
    }

    public async Task<IActionResult> OnPostPublishAsync()
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

        var result = await _eventsService.UpdateStatusAsync(ViewModel.Id, EventStatus.Approved);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage("/Management/EventDetail", new { id = ViewModel.Id });
        }

        TempData["SuccessMessage"] = "Event published successfully! Push notifications have been sent to students.";
        return RedirectToPage("/Management/EventDetail", new { id = ViewModel.Id });
    }
}
