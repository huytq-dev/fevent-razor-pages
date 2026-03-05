namespace Domain;

public class Club : EntityBase<Guid>, ISoftDeletable
{
    public required string Name { get; set; }
    public string? Code { get; set; } 
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? SocialLink { get; set; } // Facebook/Web link

    // Quản lý trạng thái
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    // Relationship
    public virtual ICollection<ClubMember> Members { get; set; } = new List<ClubMember>();
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
