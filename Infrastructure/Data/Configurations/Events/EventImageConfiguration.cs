namespace Infrastructure;

public class EventImageConfiguration : BaseEntityConfiguration<EventImage, Guid>
{
    public override void Configure(EntityTypeBuilder<EventImage> builder)
    {
        base.Configure(builder);

        builder.ToTable("EventImages");

        // --- Properties ---
        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Caption)
            .HasMaxLength(200);

        // --- Relationships ---
        builder.HasOne(e => e.Event)
            .WithMany(e => e.Images)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Indexes ---
        builder.HasIndex(e => e.EventId);
    }
}