namespace Infrastructure;

public class UserRoleConfiguration : BaseEntityConfiguration<UserRole, Guid>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserRoles");

        // --- Relationships ---
        // Restrict từ User để tránh multiple cascade paths (bảng trung gian)
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // --- Indexes ---
        // Đảm bảo 1 user không bị gán trùng 1 role nhiều lần
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

        builder.HasIndex(ur => ur.UserId);
        builder.HasIndex(ur => ur.RoleId);
    }
}
