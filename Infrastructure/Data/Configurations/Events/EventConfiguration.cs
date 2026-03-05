namespace Infrastructure;

public class EventConfiguration : BaseEntityConfiguration<Event, Guid>
{
    public override void Configure(EntityTypeBuilder<Event> builder)
    {
        base.Configure(builder);

        builder.ToTable("Events");

        // --- Properties ---
        builder.Property(e => e.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.IsPrivate)
            .HasDefaultValue(false);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        // --- Relationships ---
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Location)
            .WithMany(l => l.Events)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Organizer)
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Indexes ---
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.OrganizerId);
        builder.HasIndex(e => e.StartTime);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.ClubId);
    }
}
