using Application;
using Contract;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Admin;

public class EventApprovalModel(IEventsService eventsService) : PageModel
{
    private const int PageSizeDefault = 10;

    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public IReadOnlyList<EventSummaryResponse> Events { get; private set; } = [];
    public int TotalCount { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (TempData["SuccessMessage"] is string s) SuccessMessage = s;
        if (TempData["ErrorMessage"]   is string e) ErrorMessage   = e;
        await LoadEventsAsync(ct);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid eventId, CancellationToken ct)
    {
        var result = await eventsService.UpdateStatusAsync(eventId, EventStatus.Approved, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
            result.IsSuccess ? "Event approved." : result.Message;
        return RedirectToPage(new { StatusFilter, Search, PageNumber });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid eventId, CancellationToken ct)
    {
        var result = await eventsService.UpdateStatusAsync(eventId, EventStatus.Rejected, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
            result.IsSuccess ? "Event rejected." : result.Message;
        return RedirectToPage(new { StatusFilter, Search, PageNumber });
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid eventId, CancellationToken ct)
    {
        var result = await eventsService.UpdateStatusAsync(eventId, EventStatus.Cancelled, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
            result.IsSuccess ? "Event cancelled." : result.Message;
        return RedirectToPage(new { StatusFilter, Search, PageNumber });
    }

    private async Task LoadEventsAsync(CancellationToken ct)
    {
        var page = Math.Max(1, PageNumber);

        int? statusInt = null;
        if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<EventStatus>(StatusFilter, out var fs))
            statusInt = (int)fs;

        var query = new QueryInfo
        {
            Top = PageSizeDefault,
            Skip = (page - 1) * PageSizeDefault,
            OrderBy = "createdAt desc",
            NeedTotalCount = true,
            IsActive = false,
            SearchText = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            Status = statusInt,
        };

        var result = await eventsService.GetAllAsync(query, ct);
        if (!result.IsSuccess || result.Data is null) return;

        Events      = result.Data.Items;
        TotalCount  = result.Data.TotalCount ?? 0;
        CurrentPage = page;
        TotalPages  = TotalCount == 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSizeDefault);
    }
}
