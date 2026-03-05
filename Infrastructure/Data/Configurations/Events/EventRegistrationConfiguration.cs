namespace Infrastructure;

public class EventRegistrationConfiguration : BaseEntityConfiguration<EventRegistration, Guid>
{
    public override void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        base.Configure(builder);

        builder.ToTable("EventRegistrations");

        // --- Properties ---
        builder.Property(r => r.Status)
            .HasConversion<int>();

        builder.Property(r => r.CancellationReason)
            .HasMaxLength(500);

        // --- Relationships ---
        builder.HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany(u => u.EventRegistrations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.TicketType)
            .WithMany(t => t.Registrations)
            .HasForeignKey(r => r.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Indexes ---
        builder.HasIndex(r => new { r.EventId, r.UserId }).IsUnique();
        builder.HasIndex(r => r.EventId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.TicketTypeId);
        builder.HasIndex(r => r.Status);
    }
}
