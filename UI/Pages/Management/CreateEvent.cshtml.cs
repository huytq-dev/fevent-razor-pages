using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UI.Models.Events;

namespace UI.Pages.Management;

public class CreateEventModel : PageModel
{
    private readonly ICatalogService _catalogService;
    private readonly IEventsService _eventsService;
    private readonly IWebHostEnvironment _environment;

    public CreateEventModel(ICatalogService catalogService, IEventsService eventsService, IWebHostEnvironment environment)
    {
        _catalogService = catalogService;
        _eventsService = eventsService;
        _environment = environment;
    }

    [BindProperty]
    public CreateEventViewModel ViewModel { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Locations { get; set; } = new();
    
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        FullName = HttpContext.Session.GetString("FullName") ?? "Anonymous Organizer";
        AvatarUrl = HttpContext.Session.GetString("AvatarUrl");
        await LoadCatalogsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        FullName = HttpContext.Session.GetString("FullName") ?? "Anonymous Organizer";
        AvatarUrl = HttpContext.Session.GetString("AvatarUrl");

        if (!ModelState.IsValid)
        {
            await LoadCatalogsAsync();
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
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ViewModel.BannerImage.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ViewModel.BannerImage.CopyToAsync(fileStream);
            }

            thumbnailUrl = "/uploads/" + fileName;
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
            // ClubId would be set if the user belongs to a club
        };

        var result = await _eventsService.CreateAsync(request);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Event created successfully and is pending approval.";
            return RedirectToPage("./CreateEvent"); // Or redirect to event list
        }

        ModelState.AddModelError(string.Empty, result.Message);
        await LoadCatalogsAsync();
        return Page();
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
    }
}
