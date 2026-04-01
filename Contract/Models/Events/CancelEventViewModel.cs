using System.ComponentModel.DataAnnotations;

namespace Contract;

public class CancelEventViewModel
{
    [Required]
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int RegisteredCount { get; set; }
    public string? ThumbnailUrl { get; set; }

    [Required(ErrorMessage = "Please provide a reason for cancellation")]
    [MaxLength(500)]
    public string CancellationReason { get; set; } = string.Empty;

    [Required(ErrorMessage = "You must confirm that you understand the consequences")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm that you understand the consequences")]
    public bool ConfirmConsequences { get; set; }
}
