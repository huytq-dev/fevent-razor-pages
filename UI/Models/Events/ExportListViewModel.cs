namespace UI.Models.Events;

public class ExportHistoryViewModel
{
    public DateTime ExportTime { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string RequestedByAvatar { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ExportListViewModel
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int TotalParticipants { get; set; }

    // Configuration
    public string SelectedFormat { get; set; } = "excel";
    public List<string> SelectedFields { get; set; } = new() { "FullName", "StudentId", "Email", "Status" };
    public string StatusFilter { get; set; } = "All Participants";
    public string SortBy { get; set; } = "Name (A-Z)";

    public List<ExportHistoryViewModel> RecentExports { get; set; } = new();
}
