using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UI.Models.Events;

public class CreateEventViewModel
{
    [Required(ErrorMessage = "Event name is required")]
    [Display(Name = "Event Name")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Venue is required")]
    public Guid LocationId { get; set; }

    [Required(ErrorMessage = "Start date and time is required")]
    [Display(Name = "Start Date & Time")]
    public DateTime StartDateTime { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "End date and time is required")]
    [Display(Name = "End Date & Time")]
    public DateTime EndDateTime { get; set; } = DateTime.Now.AddHours(2);

    [Required(ErrorMessage = "Max capacity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
    [Display(Name = "Max Capacity")]
    public int MaxParticipants { get; set; }

    [Required(ErrorMessage = "Access type is required")]
    public string AccessType { get; set; } = "free";

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Banner Image")]
    public IFormFile? BannerImage { get; set; }
}
