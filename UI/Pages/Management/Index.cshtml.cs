using Application;
using Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Management;

public class IndexModel(IEventsService _eventsService) : PageModel
{
    public PagedResult<EventSummaryResponse> EventList { get; set; } = new PagedResult<EventSummaryResponse>();

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public string FullName { get; set; } = "Anonymous Organizer";
    public string? AvatarUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        FullName = HttpContext.Session.GetString("FullName") ?? "Anonymous Organizer";
        AvatarUrl = HttpContext.Session.GetString("AvatarUrl");

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login", new { ReturnUrl = "/Management/Index" });
        }

        var pageSize = 10;
        var queryInfo = new QueryInfo
        {
            Skip = (CurrentPage < 1 ? 0 : CurrentPage - 1) * pageSize,
            Top = pageSize,
            NeedTotalCount = true,
            OrganizerId = userId
        };

        var response = await _eventsService.GetAllAsync(queryInfo);
        
        if (response.IsSuccess && response.Data != null)
        {
            EventList = response.Data;
        }

        return Page();
    }
}
