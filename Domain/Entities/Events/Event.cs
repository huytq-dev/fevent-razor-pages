namespace Domain;

public class Event : EntityBase<Guid>, ISoftDeletable
{
    // Basic
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Time & Location
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    // Config
    public int MaxParticipants { get; set; }
    public EventStatus Status { get; set; }
    public bool IsPrivate { get; set; }

    // FK
    public Guid CategoryId { get; set; }
    public Guid LocationId { get; set; }
    public Guid OrganizerId { get; set; }
    public Guid? ClubId { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation
    public virtual Category Category { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual User Organizer { get; set; } = null!;
    public virtual Club? Club { get; set; }
    public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    public virtual ICollection<EventCollaborator> Collaborators { get; set; } = new List<EventCollaborator>();
    public virtual ICollection<EventImage> Images { get; set; } = new List<EventImage>();
    public virtual ICollection<EventReview> Reviews { get; set; } = new List<EventReview>();
    public virtual ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();

}
