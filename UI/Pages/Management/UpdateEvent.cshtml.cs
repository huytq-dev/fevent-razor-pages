using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Domain;
using UI.Models.Events;

namespace UI.Pages.Management;

public class UpdateEventModel : PageModel
{
    private readonly ICatalogService _catalogService;
    private readonly IEventsService _eventsService;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateEventModel(ICatalogService catalogService, IEventsService eventsService, ICloudinaryService cloudinaryService)
    {
        _catalogService = catalogService;
        _eventsService = eventsService;
        _cloudinaryService = cloudinaryService;
    }

    [BindProperty]
    public UpdateEventViewModel ViewModel { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Locations { get; set; } = new();
    public List<SelectListItem> Majors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/CreateEvent?tab=events" });
        }

        await LoadCatalogsAsync();

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

        var status = (EventStatus)eventDetail.Status;
        if (status is EventStatus.Completed)
        {
            TempData["ErrorMessage"] = "Sự kiện ở trạng thái hiện tại không thể cập nhật.";
            return RedirectToPage("/Management/EventDetail", new { id = eventDetail.Id });
        }

        // Map domain to ViewModel
        ViewModel = new UpdateEventViewModel
        {
            Id = id.Value,
            Title = eventDetail.Title,
            CategoryId = eventDetail.CategoryId,
            LocationId = eventDetail.LocationId,
            MajorId = eventDetail.MajorId,
            Description = eventDetail.Description ?? string.Empty,
            ThumbnailUrl = eventDetail.ThumbnailUrl,
            IsPublic = !eventDetail.IsPrivate,
            MaxParticipants = eventDetail.MaxParticipants,
            StartDate = eventDetail.StartTime.LocalDateTime.Date,
            StartTime = eventDetail.StartTime.LocalDateTime.TimeOfDay,
            EndTime = eventDetail.EndTime.LocalDateTime.TimeOfDay
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/CreateEvent?tab=events" });
        }

        if (!ModelState.IsValid)
        {
            await LoadCatalogsAsync();
            return Page();
        }

        var oldEvent = await _eventsService.GetDetailAsync(ViewModel.Id);
        if (!oldEvent.IsSuccess || oldEvent.Data is null)
        {
            TempData["ErrorMessage"] = oldEvent.Message;
            return RedirectToPage("/Management/CreateEvent", new { tab = "events" });
        }

        if (oldEvent.Data.OrganizerId != userId)
        {
            return Forbid();
        }

        var startDateTime = ViewModel.StartDate.Date + ViewModel.StartTime;
        var endDateTime = ViewModel.StartDate.Date + ViewModel.EndTime;
        string? thumbnailUrl = oldEvent.Data.ThumbnailUrl;

        if (ViewModel.CoverImage is not null)
        {
            thumbnailUrl = await _cloudinaryService.UploadImageAsync(ViewModel.CoverImage, "fevent-thumbnails");
        }

        var request = new UpdateEventRequest
        {
            Id = ViewModel.Id,
            Title = ViewModel.Title,
            Description = ViewModel.Description,
            ThumbnailUrl = thumbnailUrl,
            StartTime = startDateTime,
            EndTime = endDateTime,
            MaxParticipants = ViewModel.MaxParticipants,
            CategoryId = ViewModel.CategoryId,
            LocationId = ViewModel.LocationId,
            MajorId = ViewModel.MajorId,
            IsPrivate = !ViewModel.IsPublic,
            OrganizerId = userId
        };

        var result = await _eventsService.UpdateAsync(request);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            await LoadCatalogsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Event updated successfully.";
        return RedirectToPage("/Management/EventDetail", new { id = ViewModel.Id });
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

        var majors = await _catalogService.GetMajorsAsync();
        Majors = majors.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = string.IsNullOrEmpty(m.Code) ? m.Name : $"[{m.Code}] {m.Name}",
            Selected = m.Id == ViewModel.MajorId
        }).ToList();
    }
}
