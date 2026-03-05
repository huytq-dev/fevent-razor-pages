namespace Infrastructure;

public class LocationConfiguration : BaseEntityConfiguration<Location, Guid>
{
    public override void Configure(EntityTypeBuilder<Location> builder)
    {
        base.Configure(builder);

        builder.ToTable("Locations");

        // --- Properties ---
        builder.Property(l => l.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Address)
            .HasMaxLength(255);

        builder.Property(l => l.MapUrl)
            .HasMaxLength(500);

        // --- Indexes ---
        builder.HasIndex(l => l.Name);
    }
}
