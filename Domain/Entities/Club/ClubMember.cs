namespace Domain;

public class ClubMember : EntityBase<Guid>
{
    public Guid ClubId { get; set; }
    public Guid UserId { get; set; }
    public ClubMemberRole Role { get; set; }
    public ClubMemberStatus Status { get; set; }

    // Navigation Property
    public virtual Club Club { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}