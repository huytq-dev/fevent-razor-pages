using System.ComponentModel.DataAnnotations;

namespace UI.Models.Events;

public class DeleteEventViewModel
{
    [Required]
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }

    [Required(ErrorMessage = "You must type DELETE to confirm")]
    public string DeleteConfirmation { get; set; } = string.Empty;
}
