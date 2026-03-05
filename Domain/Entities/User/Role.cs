namespace Domain;

public class Role : EntityBase<Guid>
{
    public required string RoleName { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
