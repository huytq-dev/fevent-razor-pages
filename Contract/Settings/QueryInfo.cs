namespace Contract;

public class QueryInfo
{
    public int Top { get; set; } = AppConstants.DefaultPageTop;
    public int Skip { get; set; } = AppConstants.DefaultPageSkip;
    public string? OrderBy { get; set; } = AppConstants.DefaultOrderBy;
    public bool NeedTotalCount { get; set; } = AppConstants.DefaultNeedTotalCount;
    public bool IsActive { get; set; } = true;
    public string? SearchText { get; set; }

    /// <summary>Filter by category IDs (OR logic when multiple).</summary>
    public IReadOnlyList<Guid>? CategoryIds { get; set; }

    /// <summary>Filter by location IDs (OR logic when multiple).</summary>
    public IReadOnlyList<Guid>? LocationIds { get; set; }

    /// <summary>Filter events starting on or after this date (inclusive).</summary>
    public DateTime? StartDateFrom { get; set; }

    /// <summary>Filter events starting on or before this date (inclusive).</summary>
    public DateTime? StartDateTo { get; set; }

    /// <summary>Filter events by a specific organizer.</summary>
    public Guid? OrganizerId { get; set; }
}
