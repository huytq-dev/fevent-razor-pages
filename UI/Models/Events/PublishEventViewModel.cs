using System.ComponentModel.DataAnnotations;

namespace UI.Models.Events;

public class PublishEventViewModel
{
    [Required]
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    
    // Status and visibility
    public bool IsPrivate { get; set; }
    public string Status { get; set; } = string.Empty;

    // Validation checklist (Mocked for UI)
    public List<string> Checks { get; set; } = new();

    [Required(ErrorMessage = "You must acknowledge the publications warning")]
    public bool AcknowledgeWarning { get; set; }
}
