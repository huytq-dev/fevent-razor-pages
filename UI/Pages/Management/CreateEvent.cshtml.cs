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

        MyEvents = result.Data?.Items.ToList() ?? new();
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
