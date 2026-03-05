namespace Domain;

public class User : EntityBase<Guid>, ISoftDeletable
{
    // Basic Info
    public required string FullName { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; } 
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Major { get; set; }

    // FPT/Student Info
    public string? StudentId { get; set; }
    public string? SchoolName { get; set; }   

    // Demographics
    public GenderType? Gender { get; set; }
    public DateTime? DOB { get; set; }
    public string? Address { get; set; }

    // System Status
    public bool IsVerified { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    // JWT
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }

    // Navigation
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<SocialLink> SocialLinks { get; set; } = new List<SocialLink>();
    public virtual ICollection<Event> OrganizedEvents { get; set; } = new List<Event>(); 
    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>(); 
    public virtual ICollection<EventCollaborator> EventCollaborations { get; set; } = new List<EventCollaborator>();
    public virtual ICollection<EventReview> Reviews { get; set; } = new List<EventReview>(); 
}
