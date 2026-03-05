namespace Infrastructure;

public class EventCollaboratorConfiguration : BaseEntityConfiguration<EventCollaborator, Guid>
{
    public override void Configure(EntityTypeBuilder<EventCollaborator> builder)
    {
        base.Configure(builder);

        builder.ToTable("EventCollaborators");

        // --- Properties ---
        builder.Property(e => e.Role)
            .HasConversion<int>()
            .IsRequired();

        // --- Relationships ---
        // Restrict để tránh multiple cascade paths từ User và Event đến bảng trung gian
        builder.HasOne(e => e.Event)
            .WithMany(e => e.Collaborators)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(u => u.EventCollaborations)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Indexes ---
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.EventId, e.UserId }).IsUnique();
        builder.HasIndex(e => e.Role);
    }
}
