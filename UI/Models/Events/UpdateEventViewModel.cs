using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UI.Models.Events;

public class UpdateEventViewModel : IValidatableObject
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Event name is required")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "Event name must be between 5 and 120 characters")]
    [Display(Name = "Event Name")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public Guid LocationId { get; set; }

    public Guid? MajorId { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(4000, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 4000 characters")]
    public string Description { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    [Display(Name = "Cover Image")]
    public IFormFile? CoverImage { get; set; }

    [Display(Name = "Public visibility")]
    public bool IsPublic { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Start time is required")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(9);

    [Required(ErrorMessage = "End time is required")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(12);

    [Required(ErrorMessage = "Max capacity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
    public int MaxParticipants { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var startAt = StartDate.Date + StartTime;
        var endAt = StartDate.Date + EndTime;

        if (startAt < DateTime.Now)
        {
            yield return new ValidationResult("Start time cannot be in the past.", new[] { nameof(StartDate), nameof(StartTime) });
        }

        if (endAt <= startAt)
        {
            yield return new ValidationResult("End time must be after start time.", new[] { nameof(EndTime) });
        }

    }
}
