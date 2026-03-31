namespace UI;

public class FindEventModel(IEventsService eventsService, ICatalogService catalogService) : PageModel
{
    private const int PageSizeDefault = 9;

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = PageSizeDefault;
    [BindProperty(SupportsGet = true)] public string? OrderBy { get; set; }
    [BindProperty(SupportsGet = true)] public List<Guid> CategoryIds { get; set; } = [];
    [BindProperty(SupportsGet = true)] public List<Guid> LocationIds { get; set; } = [];
    [BindProperty(SupportsGet = true)] public Guid? MajorId { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? StartDateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? StartDateTo { get; set; }

    public IReadOnlyList<EventSummaryResponse> Events { get; private set; } = [];
    public IReadOnlyList<CategoryFilterItem> Categories { get; private set; } = [];
    public IReadOnlyList<LocationFilterItem> Locations { get; private set; } = [];
    public IReadOnlyList<MajorFilterItem> Majors { get; private set; } = [];
    public int TotalCount { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var page = Math.Max(1, PageNumber);
        var size = Math.Clamp(PageSize < 1 ? PageSizeDefault : PageSize, 1, AppConstants.MaxPageSize);

        var query = new QueryInfo
        {
            Top = size,
            Skip = (page - 1) * size,
            OrderBy = string.IsNullOrWhiteSpace(OrderBy) ? AppConstants.DefaultOrderBy : OrderBy,
            NeedTotalCount = true,
            SearchText = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            CategoryIds = CategoryIds.Count > 0 ? CategoryIds : null,
            LocationIds = LocationIds.Count > 0 ? LocationIds : null,
            MajorId = MajorId,
            StartDateFrom = StartDateFrom?.ToDateTime(TimeOnly.MinValue),
            StartDateTo = StartDateTo?.ToDateTime(TimeOnly.MinValue)
        };

        Categories = await catalogService.GetCategoriesAsync(ct);
        Locations = await catalogService.GetLocationsAsync(ct);
        Majors = await catalogService.GetMajorsAsync(ct);

        var result = await eventsService.GetAllAsync(query, ct);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        Events = result.Data.Items;
        TotalCount = result.Data.TotalCount ?? 0;
        CurrentPage = page;
        TotalPages = TotalCount == 0 ? 1 : (int)Math.Ceiling((double)TotalCount / size);
    }
}
