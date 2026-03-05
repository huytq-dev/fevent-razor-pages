namespace Domain;

public class UserRole : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
