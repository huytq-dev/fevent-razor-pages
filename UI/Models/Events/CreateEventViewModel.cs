using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UI.Models.Events;

public class CreateEventViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Event name is required")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "Event name must be between 5 and 120 characters")]
    [Display(Name = "Event Name")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid? CategoryId { get; set; }

    [Required(ErrorMessage = "Venue is required")]
    public Guid? LocationId { get; set; }

    [Required(ErrorMessage = "Start date and time is required")]
    [Display(Name = "Start Date & Time")]
    public DateTime? StartDateTime { get; set; }

    [Required(ErrorMessage = "End date and time is required")]
    [Display(Name = "End Date & Time")]
    public DateTime? EndDateTime { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDateTime.HasValue && StartDateTime.Value < DateTime.Now)
            yield return new ValidationResult("Start time cannot be in the past.", new[] { nameof(StartDateTime) });

        if (StartDateTime.HasValue && EndDateTime.HasValue && EndDateTime.Value <= StartDateTime.Value)
            yield return new ValidationResult("End time must be after start time.", new[] { nameof(EndDateTime) });

        var allowedAccessTypes = new[] { "free", "paid", "invite" };
        if (!allowedAccessTypes.Contains(AccessType))
            yield return new ValidationResult("Invalid access type.", new[] { nameof(AccessType) });
    }

    [Required(ErrorMessage = "Max capacity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
    [Display(Name = "Max Capacity")]
    public int MaxParticipants { get; set; }

    [Required(ErrorMessage = "Access type is required")]
    public string AccessType { get; set; } = "free";

    [Required(ErrorMessage = "Description is required")]
    [StringLength(4000, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 4000 characters")]
    public string Description { get; set; } = string.Empty;

    public Guid? MajorId { get; set; }

    [Display(Name = "Banner Image")]
    public IFormFile? BannerImage { get; set; }
}
