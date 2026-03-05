namespace Contract;

public class QueryInfo
{
    public int Top { get; set; } = AppConstants.DefaultPageTop;
    public int Skip { get; set; } = AppConstants.DefaultPageSkip;
    public string? OrderBy { get; set; } = AppConstants.DefaultOrderBy;
    public bool NeedTotalCount { get; set; } = AppConstants.DefaultNeedTotalCount;
    public bool IsActive { get; set; } = true;
    public string? SearchText { get; set; }
}
