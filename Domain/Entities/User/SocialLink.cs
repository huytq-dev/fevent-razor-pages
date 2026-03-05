namespace Domain;

public class SocialLink : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public SocialPlatformType Platform { get; set; }
    public string Url { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
}
