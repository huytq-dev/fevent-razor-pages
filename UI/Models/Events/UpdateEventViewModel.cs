using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UI.Models.Events;

public class UpdateEventViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Event name is required")]
    [Display(Name = "Event Name")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public Guid LocationId { get; set; }

    public Guid? MajorId { get; set; }

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    [Display(Name = "Cover Image")]
    public IFormFile? CoverImage { get; set; }

    [Display(Name = "Published Status")]
    public bool IsPublished { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Start time is required")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(9);

    [Required(ErrorMessage = "End time is required")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(12);

    [Required(ErrorMessage = "Registration deadline is required")]
    [DataType(DataType.Date)]
    public DateTime RegistrationDeadline { get; set; } = DateTime.Today.AddDays(-1);

    [Required(ErrorMessage = "Max capacity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
    public int MaxParticipants { get; set; }
}
