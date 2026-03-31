using Application;
using Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UI.Models.Events;

namespace UI.Pages.Management;

public class CreateEventModel : PageModel
{
    private readonly ICatalogService _catalogService;
    private readonly IEventsService _eventsService;
    private readonly ICloudinaryService _cloudinaryService;

    public CreateEventModel(ICatalogService catalogService, IEventsService eventsService, ICloudinaryService cloudinaryService)
    {
        _catalogService = catalogService;
        _eventsService = eventsService;
        _cloudinaryService = cloudinaryService;
    }

    [BindProperty]
    public CreateEventViewModel ViewModel { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? EventSearch { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Locations { get; set; } = new();
    public List<SelectListItem> Majors { get; set; } = new();

    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public List<EventSummaryResponse> MyEvents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        FullName = HttpContext.Session.GetString("FullName") ?? "Anonymous Organizer";
        AvatarUrl = HttpContext.Session.GetString("AvatarUrl");
        await LoadCatalogsAsync();
        await LoadMyEventsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        FullName = HttpContext.Session.GetString("FullName") ?? "Anonymous Organizer";
        AvatarUrl = HttpContext.Session.GetString("AvatarUrl");

        if (!ModelState.IsValid)
        {
            await LoadCatalogsAsync();
            await LoadMyEventsAsync();
            return Page();
        }

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/CreateEvent" });
        }

        string? thumbnailUrl = null;
        if (ViewModel.BannerImage != null)
        {
            thumbnailUrl = await _cloudinaryService.UploadImageAsync(ViewModel.BannerImage, "fevent-thumbnails");
        }

        var request = new CreateEventRequest
        {
            Title = ViewModel.Title,
            Description = ViewModel.Description,
            ThumbnailUrl = thumbnailUrl,
            StartTime = ViewModel.StartDateTime!.Value,
            EndTime = ViewModel.EndDateTime!.Value,
            MaxParticipants = ViewModel.MaxParticipants,
            CategoryId = ViewModel.CategoryId!.Value,
            LocationId = ViewModel.LocationId!.Value,
            OrganizerId = userId,
            MajorId = ViewModel.MajorId,
        };

        var result = await _eventsService.CreateAsync(request);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Event created successfully and is pending approval.";
            return RedirectToPage("./CreateEvent");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        await LoadCatalogsAsync();
        await LoadMyEventsAsync();
        return Page();
    }

    private async Task LoadMyEventsAsync()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!Guid.TryParse(userIdStr, out var userId)) return;

        var result = await _eventsService.GetAllAsync(new QueryInfo
        {
            OrganizerId = userId,
            Top = 50,
            Skip = 0,
            IsActive = true,
            NeedTotalCount = false,
            OrderBy = "createdAt desc"
        });

        var events = result.Data?.Items.ToList() ?? new();

        if (!string.IsNullOrWhiteSpace(EventSearch))
        {
            var term = EventSearch.Trim();
            events = events.Where(e =>
                e.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (e.LocationName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.CategoryName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        MyEvents = events;
    }

    public async Task<IActionResult> OnGetExportMyEventsAsync()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/CreateEvent?tab=events" });
        }

        var result = await _eventsService.GetAllAsync(new QueryInfo
        {
            OrganizerId = userId,
            Top = 1000,
            Skip = 0,
            IsActive = true,
            NeedTotalCount = false,
            OrderBy = "createdAt desc"
        });

        var items = result.Data?.Items ?? [];
        if (!string.IsNullOrWhiteSpace(EventSearch))
        {
            var term = EventSearch.Trim();
            items = items.Where(e =>
                e.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (e.LocationName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.CategoryName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        var rows = items.Select((ev, i) => new[]
        {
            (i + 1).ToString(),
            ev.Title,
            ev.CategoryName,
            ev.LocationName,
            ev.StartTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            ev.EndTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm"),
            ((Domain.EventStatus)ev.Status).ToString(),
            ev.RegisteredCount.ToString(),
            ev.MaxParticipants.ToString(),
            ev.ThumbnailUrl
        });

        var csv = UI.Helpers.CsvExportHelper.BuildCsv(
            ["No", "Title", "Category", "Location", "Start", "End", "Status", "Registered", "Capacity", "Thumbnail URL"],
            rows);

        var fileName = $"my-events-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    private async Task LoadCatalogsAsync()
    {
        var categories = await _catalogService.GetCategoriesAsync();
        Categories = categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();

        var locations = await _catalogService.GetLocationsAsync();
        Locations = locations.Select(l => new SelectListItem
        {
            Value = l.Id.ToString(),
            Text = l.Name
        }).ToList();

        var majors = await _catalogService.GetMajorsAsync();
        Majors = majors.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = string.IsNullOrEmpty(m.Code) ? m.Name : $"[{m.Code}] {m.Name}"
        }).ToList();
    }
}
