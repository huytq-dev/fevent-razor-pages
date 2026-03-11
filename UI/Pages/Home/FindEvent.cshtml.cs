namespace UI;

public class FindEventModel(IEventsService eventsService, ICatalogService catalogService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(Name = "Page", SupportsGet = true)]
    public int PageNumber { get; set; } = AppConstants.DefaultPage;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = AppConstants.DefaultPageTop;

    [BindProperty(SupportsGet = true)]
    public string? OrderBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<Guid> CategoryIds { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public List<Guid> LocationIds { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDateTo { get; set; }

    public IReadOnlyList<EventSummaryResponse> Events { get; private set; } = Array.Empty<EventSummaryResponse>();
    public int? TotalCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    public IReadOnlyList<CategoryFilterItem> Categories { get; private set; } = [];
    public IReadOnlyList<LocationFilterItem> Locations { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        var page = PageNumber < 1 ? 1 : PageNumber;
        var pageSize = PageSize < 1 ? AppConstants.DefaultPageTop : PageSize;
        if (pageSize > AppConstants.MaxPageSize) pageSize = AppConstants.MaxPageSize;

        var query = new QueryInfo
        {
            Top = pageSize,
            Skip = (page - 1) * pageSize,
            OrderBy = string.IsNullOrWhiteSpace(OrderBy) ? AppConstants.DefaultOrderBy : OrderBy,
            NeedTotalCount = true,
            SearchText = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            CategoryIds = CategoryIds.Count > 0 ? CategoryIds : null,
            LocationIds = LocationIds.Count > 0 ? LocationIds : null,
            StartDateFrom = StartDateFrom?.ToDateTime(TimeOnly.MinValue),
            StartDateTo = StartDateTo?.ToDateTime(TimeOnly.MinValue)
        };

        Categories = await catalogService.GetCategoriesAsync(ct);
        Locations = await catalogService.GetLocationsAsync(ct);

        var result = await eventsService.GetAllAsync(query, ct);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            Events = Array.Empty<EventSummaryResponse>();
            TotalCount = 0;
            return;
        }

        Events = result.Data.Items;
        TotalCount = result.Data.TotalCount;
    }
}
