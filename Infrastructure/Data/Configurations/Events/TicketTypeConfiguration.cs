namespace Infrastructure;

public class TicketTypeConfiguration : BaseEntityConfiguration<TicketType, Guid>
{
    public override void Configure(EntityTypeBuilder<TicketType> builder)
    {
        base.Configure(builder);

        builder.ToTable("TicketTypes");

        // --- Properties ---
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Price)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(e => e.Quantity)
            .HasDefaultValue(0);

        builder.Property(e => e.SoldCount)
            .HasDefaultValue(0);

        // --- Relationships ---
        builder.HasOne(e => e.Event)
            .WithMany(e => e.TicketTypes)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Indexes ---
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => new { e.EventId, e.Name }).IsUnique();
    }
}