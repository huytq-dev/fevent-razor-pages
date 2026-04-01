namespace Contract;

public class ParticipantViewModel
{
    public int Order { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Mssv { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string Status { get; set; } = string.Empty; // Registered, Checked-in, Cancelled
    public DateTime? CheckInTime { get; set; }
    public string Initials =>
        string.IsNullOrWhiteSpace(FullName)
            ? "?"
            : string.Join("", FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]));
}

public class ParticipantListViewModel
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int TotalRegistered { get; set; }
    public List<ParticipantViewModel> Participants { get; set; } = new();

    /// <summary>Distinct majors from current registrations (for filter dropdown).</summary>
    public List<string> MajorFilterOptions { get; set; } = new();
}
