namespace UI.Models.Events;

public class ParticipantViewModel
{
    public int Order { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Mssv { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string Status { get; set; } = string.Empty; // Registered, Checked-in, Cancelled
    public string Initials => string.Join("", FullName.Split(' ').Select(s => s[0]));
}

public class ParticipantListViewModel
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int TotalRegistered { get; set; }
    public List<ParticipantViewModel> Participants { get; set; } = new();
    
    // Filter properties
    public string? SearchQuery { get; set; }
    public string? StatusFilter { get; set; }
}
