using Application;
using Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UI.Helpers;
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
    public bool IsPreviewMode { get; set; }
    public EventPreviewCard? Preview { get; set; }

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
            TempData["InfoMessage"] = "Bạn có thể vào Dashboard để quản lý, cập nhật, publish hoặc hủy sự kiện.";
            return RedirectToPage("./CreateEvent");
        }

        TempData["ErrorMessage"] = result.Message;
        ModelState.AddModelError(string.Empty, result.Message);
        await LoadCatalogsAsync();
        await LoadMyEventsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        FullName = HttpContext.Session.GetString("FullName") ?? "Anonymous Organizer";
        AvatarUrl = HttpContext.Session.GetString("AvatarUrl");

        await LoadCatalogsAsync();
        await LoadMyEventsAsync();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Vui lòng nhập đủ thông tin hợp lệ trước khi xem preview.";
            return Page();
        }

        IsPreviewMode = true;
        Preview = new EventPreviewCard
        {
            Title = ViewModel.Title,
            Description = ViewModel.Description,
            StartTime = ViewModel.StartDateTime ?? DateTime.Now,
            EndTime = ViewModel.EndDateTime ?? DateTime.Now,
            MaxParticipants = ViewModel.MaxParticipants,
            CategoryName = Categories.FirstOrDefault(c => c.Value == ViewModel.CategoryId?.ToString())?.Text ?? "N/A",
            LocationName = Locations.FirstOrDefault(c => c.Value == ViewModel.LocationId?.ToString())?.Text ?? "N/A",
            MajorName = ViewModel.MajorId.HasValue
                ? Majors.FirstOrDefault(c => c.Value == ViewModel.MajorId.Value.ToString())?.Text ?? "N/A"
                : "All majors",
            AccessType = ViewModel.AccessType,
            BannerFileName = ViewModel.BannerImage?.FileName
        };

        TempData["InfoMessage"] = "Đây là bản xem trước sự kiện. Dữ liệu chưa được tạo cho đến khi bấm Submit for Approval.";
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

        var workbook = ExcelExportHelper.BuildWorkbook(
            ["No", "Title", "Category", "Location", "Start", "End", "Status", "Registered", "Capacity", "Thumbnail URL"],
            rows,
            "My Events");

        var fileName = $"my-events-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
            Text = NormalizeLocationName(l.Name)
        }).ToList();

        var majors = await _catalogService.GetMajorsAsync();
        Majors = majors.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = string.IsNullOrEmpty(m.Code) ? m.Name : $"[{m.Code}] {m.Name}"
        }).ToList();
    }

    private static string NormalizeLocationName(string name)
    {
        if (name.Contains("Can Tho", StringComparison.OrdinalIgnoreCase) && !name.Contains("Campus", StringComparison.OrdinalIgnoreCase))
            return "FPT Cần Thơ Campus";
        if (name.Contains("Da Nang", StringComparison.OrdinalIgnoreCase) && !name.Contains("Campus", StringComparison.OrdinalIgnoreCase))
            return "FPT Đà Nẵng Campus";
        if (name.Contains("Quy Nhon", StringComparison.OrdinalIgnoreCase) && !name.Contains("Campus", StringComparison.OrdinalIgnoreCase))
            return "FPT Quy Nhơn Campus";
        if (name.Contains("HCM", StringComparison.OrdinalIgnoreCase) && !name.Contains("Campus", StringComparison.OrdinalIgnoreCase))
            return "FPT HCM Campus";
        if (name.Contains("Hanoi", StringComparison.OrdinalIgnoreCase) && !name.Contains("Campus", StringComparison.OrdinalIgnoreCase))
            return "FPT Hà Nội Campus";

        return name;
    }

    public sealed class EventPreviewCard
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxParticipants { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty;
        public string? BannerFileName { get; set; }
    }
}
