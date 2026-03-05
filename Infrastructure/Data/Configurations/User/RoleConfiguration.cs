namespace Infrastructure;

public class RoleConfiguration : BaseEntityConfiguration<Role, Guid>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("Roles");

        // --- Properties ---
        builder.Property(r => r.RoleName)
            .HasMaxLength(50)
            .IsRequired();

        // --- Indexes ---
        builder.HasIndex(r => r.RoleName).IsUnique();
    }
}
